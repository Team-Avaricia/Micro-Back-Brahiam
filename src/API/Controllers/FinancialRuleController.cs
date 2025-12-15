using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialRuleController : ControllerBase
    {
        private readonly IFinancialRuleRepository _ruleRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;

        public FinancialRuleController(
            IFinancialRuleRepository ruleRepository, 
            ITransactionRepository transactionRepository,
            IUserRepository userRepository)
        {
            _ruleRepository = ruleRepository;
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Resolves a user identifier (GUID or email) to a GUID
        /// </summary>
        private async Task<Guid?> ResolveUserIdAsync(string userIdentifier)
        {
            // Try to parse as GUID first
            if (Guid.TryParse(userIdentifier, out var guid))
            {
                return guid;
            }

            // If not a GUID, treat as email
            var user = await _userRepository.GetByEmailAsync(userIdentifier);
            return user?.Id;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRule([FromBody] CreateFinancialRuleRequest request)
        {
            try
            {
                var userId = await ResolveUserIdAsync(request.UserId);
                if (userId == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                var rule = new FinancialRule(userId.Value, request.Type, request.Category, request.AmountLimit, request.Period);
                
                await _ruleRepository.AddAsync(rule);
                
                return Ok(new
                {
                    id = rule.Id,
                    userId = rule.UserId,
                    type = rule.Type.ToString(),
                    category = rule.Category,
                    amountLimit = rule.AmountLimit,
                    period = rule.Period.ToString(),
                    isActive = rule.IsActive,
                    createdAt = rule.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRules(string userId)
        {
            try
            {
                var userGuid = await ResolveUserIdAsync(userId);
                if (userGuid == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                var rules = await _ruleRepository.GetActiveRulesByUserIdAsync(userGuid.Value);
                
                // Map to DTO to avoid circular reference issues
                var response = rules.Select(r => new
                {
                    id = r.Id,
                    userId = r.UserId,
                    type = r.Type.ToString(),
                    category = r.Category,
                    amountLimit = r.AmountLimit,
                    period = r.Period.ToString(),
                    isActive = r.IsActive,
                    createdAt = r.CreatedAt,
                    updatedAt = r.UpdatedAt
                });
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRule(string id)
        {
            try
            {
                var ruleGuid = Guid.Parse(id);
                var rule = await _ruleRepository.GetByIdAsync(ruleGuid);
                
                if (rule == null)
                    return NotFound(new { error = "Rule not found" });

                // Map to DTO to avoid circular reference issues
                var response = new
                {
                    id = rule.Id,
                    userId = rule.UserId,
                    type = rule.Type.ToString(),
                    category = rule.Category,
                    amountLimit = rule.AmountLimit,
                    period = rule.Period.ToString(),
                    isActive = rule.IsActive,
                    createdAt = rule.CreatedAt,
                    updatedAt = rule.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateRule(string id)
        {
            try
            {
                var ruleGuid = Guid.Parse(id);
                var rule = await _ruleRepository.GetByIdAsync(ruleGuid);

                if (rule == null)
                    return NotFound(new { error = "Rule not found" });

                rule.Deactivate();
                await _ruleRepository.UpdateAsync(rule);

                return Ok(new { message = "Rule deactivated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRule(string id)
        {
            try
            {
                var ruleGuid = Guid.Parse(id);
                var rule = await _ruleRepository.GetByIdAsync(ruleGuid);

                if (rule == null)
                    return NotFound(new { error = "Rule not found" });

                await _ruleRepository.DeleteAsync(ruleGuid);

                return Ok(new { message = "Rule deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the progress of a financial rule, showing how much has been spent
        /// in the current period and how much budget remains.
        /// </summary>
        [HttpGet("{id}/progress")]
        public async Task<IActionResult> GetRuleProgress(string id)
        {
            try
            {
                var ruleGuid = Guid.Parse(id);
                var rule = await _ruleRepository.GetByIdAsync(ruleGuid);

                if (rule == null)
                    return NotFound(new { error = "Rule not found" });

                // Calculate period dates based on rule period type
                var (periodStart, periodEnd) = CalculatePeriodDates(rule.Period);

                // Get transactions for this user, category, and period
                var transactions = await _transactionRepository.GetByUserAndPeriodAsync(
                    rule.UserId, 
                    periodStart, 
                    periodEnd, 
                    rule.Category == "General" ? null : rule.Category
                );

                // Sum only expenses
                decimal spent = 0;
                foreach (var tx in transactions)
                {
                    if (tx.Type == TransactionType.Expense)
                    {
                        spent += tx.Amount;
                    }
                }

                var remaining = rule.AmountLimit - spent;
                var percentUsed = rule.AmountLimit > 0 ? (double)(spent / rule.AmountLimit) * 100 : 0;
                var isOverBudget = spent > rule.AmountLimit;

                // Determine status
                string status;
                if (isOverBudget)
                    status = "Over Budget";
                else if (percentUsed >= 80)
                    status = "Warning";
                else
                    status = "On Track";

                var response = new RuleProgressResponse
                {
                    RuleId = rule.Id,
                    Category = rule.Category ?? "General",
                    Period = rule.Period.ToString(),
                    Limit = rule.AmountLimit,
                    Spent = spent,
                    Remaining = remaining > 0 ? remaining : 0,
                    PercentUsed = Math.Round(percentUsed, 1),
                    PeriodStartDate = periodStart,
                    PeriodEndDate = periodEnd,
                    IsOverBudget = isOverBudget,
                    Status = status
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the progress of all active rules for a user
        /// </summary>
        [HttpGet("user/{userId}/progress")]
        public async Task<IActionResult> GetAllRulesProgress(string userId)
        {
            try
            {
                var userGuid = await ResolveUserIdAsync(userId);
                if (userGuid == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                var rules = await _ruleRepository.GetActiveRulesByUserIdAsync(userGuid.Value);
                var progressList = new System.Collections.Generic.List<RuleProgressResponse>();

                foreach (var rule in rules)
                {
                    var (periodStart, periodEnd) = CalculatePeriodDates(rule.Period);

                    var transactions = await _transactionRepository.GetByUserAndPeriodAsync(
                        rule.UserId,
                        periodStart,
                        periodEnd,
                        rule.Category == "General" ? null : rule.Category
                    );

                    decimal spent = 0;
                    foreach (var tx in transactions)
                    {
                        if (tx.Type == TransactionType.Expense)
                        {
                            spent += tx.Amount;
                        }
                    }

                    var remaining = rule.AmountLimit - spent;
                    var percentUsed = rule.AmountLimit > 0 ? (double)(spent / rule.AmountLimit) * 100 : 0;
                    var isOverBudget = spent > rule.AmountLimit;

                    string status;
                    if (isOverBudget)
                        status = "Over Budget";
                    else if (percentUsed >= 80)
                        status = "Warning";
                    else
                        status = "On Track";

                    progressList.Add(new RuleProgressResponse
                    {
                        RuleId = rule.Id,
                        Category = rule.Category ?? "General",
                        Period = rule.Period.ToString(),
                        Limit = rule.AmountLimit,
                        Spent = spent,
                        Remaining = remaining > 0 ? remaining : 0,
                        PercentUsed = Math.Round(percentUsed, 1),
                        PeriodStartDate = periodStart,
                        PeriodEndDate = periodEnd,
                        IsOverBudget = isOverBudget,
                        Status = status
                    });
                }

                return Ok(progressList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Calculates the start and end dates for a given period type
        /// </summary>
        private (DateTime start, DateTime end) CalculatePeriodDates(RulePeriod period)
        {
            var now = DateTime.UtcNow;
            DateTime start, end;

            switch (period)
            {
                case RulePeriod.Daily:
                    // Start from beginning of today
                    start = DateTime.SpecifyKind(now.Date, DateTimeKind.Utc);
                    end = start.AddDays(1);
                    break;
                case RulePeriod.Weekly:
                    // Start from Monday of current week
                    int daysFromMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    start = DateTime.SpecifyKind(now.Date.AddDays(-daysFromMonday), DateTimeKind.Utc);
                    end = start.AddDays(7);
                    break;
                case RulePeriod.Biweekly:
                    // Start from 1st or 15th of month
                    if (now.Day >= 15)
                    {
                        start = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 15), DateTimeKind.Utc);
                        end = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1).AddMonths(1), DateTimeKind.Utc);
                    }
                    else
                    {
                        start = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
                        end = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 15), DateTimeKind.Utc);
                    }
                    break;
                case RulePeriod.Yearly:
                    start = DateTime.SpecifyKind(new DateTime(now.Year, 1, 1), DateTimeKind.Utc);
                    end = start.AddYears(1);
                    break;
                case RulePeriod.Monthly:
                default:
                    start = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
                    end = start.AddMonths(1);
                    break;
            }

            return (start, end);
        }
    }
}
