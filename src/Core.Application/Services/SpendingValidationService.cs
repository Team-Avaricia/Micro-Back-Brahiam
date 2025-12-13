using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;

namespace Core.Application.Services
{
    public class SpendingValidationService : ISpendingValidationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFinancialRuleRepository _ruleRepository;
        private readonly ITransactionRepository _transactionRepository;

        public SpendingValidationService(
            IUserRepository userRepository,
            IFinancialRuleRepository ruleRepository,
            ITransactionRepository transactionRepository)
        {
            _userRepository = userRepository;
            _ruleRepository = ruleRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<SpendingValidationResponse> ValidateSpendingAsync(SpendingValidationRequest request)
        {
            var userId = Guid.Parse(request.UserId);
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return CreateRejectionResponse("User not found");
            }

            if (HasInsufficientBalance(user, request.Amount))
            {
                return CreateRejectionResponse(
                    $"Insufficient balance. Available: ${user.CurrentBalance:N0}, Required: ${request.Amount:N0}", 
                    user.CurrentBalance);
            }

            var activeRules = await _ruleRepository.GetActiveRulesByUserIdAsync(userId);
            
            foreach (var rule in activeRules)
            {
                if (IsRuleApplicable(rule, request.Category))
                {
                    var violation = await CheckRuleViolationAsync(rule, userId, request.Amount);
                    if (violation != null)
                    {
                        return violation;
                    }
                }
            }

            return CreateApprovalResponse(user.CurrentBalance - request.Amount);
        }

        private bool HasInsufficientBalance(User user, decimal amount)
        {
            return user.CurrentBalance < amount;
        }

        private bool IsRuleApplicable(FinancialRule rule, string category)
        {
            return string.IsNullOrEmpty(rule.Category) || rule.Category == category;
        }

        private async Task<SpendingValidationResponse> CheckRuleViolationAsync(FinancialRule rule, Guid userId, decimal requestedAmount)
        {
            var (startDate, endDate) = GetPeriodDates(rule.Period);
            var transactionsInPeriod = await _transactionRepository.GetByUserAndPeriodAsync(
                userId, startDate, endDate, rule.Category);

            var totalSpentInPeriod = transactionsInPeriod
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var projectedTotal = totalSpentInPeriod + requestedAmount;

            if (projectedTotal > rule.AmountLimit)
            {
                var remaining = rule.AmountLimit - totalSpentInPeriod;
                return CreateRejectionResponse(
                    $"Exceeds {rule.Period} limit for {rule.Category ?? "general"}: ${rule.AmountLimit:N0}. Spent: ${totalSpentInPeriod:N0}, Remaining: ${remaining:N0}",
                    remaining);
            }

            return null;
        }

        private SpendingValidationResponse CreateRejectionResponse(string reason, decimal remainingBudget = 0)
        {
            return new SpendingValidationResponse
            {
                IsApproved = false,
                Verdict = "Rejected",
                Reason = reason,
                RemainingBudget = remainingBudget
            };
        }

        private SpendingValidationResponse CreateApprovalResponse(decimal remainingBudget)
        {
            return new SpendingValidationResponse
            {
                IsApproved = true,
                Verdict = "Approved",
                Reason = "Spending allowed",
                RemainingBudget = remainingBudget
            };
        }

        private (DateTime start, DateTime end) GetPeriodDates(RulePeriod period)
        {
            var now = DateTime.UtcNow;
            return period switch
            {
                RulePeriod.Weekly => (now.Date.AddDays(-(int)now.DayOfWeek), now.Date.AddDays(7 - (int)now.DayOfWeek)),
                RulePeriod.Biweekly => now.Day >= 15 
                    ? (new DateTime(now.Year, now.Month, 15), new DateTime(now.Year, now.Month, 1).AddMonths(1))
                    : (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 15)),
                RulePeriod.Monthly => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1)),
                RulePeriod.Yearly => (new DateTime(now.Year, 1, 1), new DateTime(now.Year + 1, 1, 1)),
                _ => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1)) // Default to Monthly
            };
        }
    }
}
