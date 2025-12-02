using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Domain.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecurringTransactionController : ControllerBase
    {
        private readonly RecurringTransactionService _recurringService;

        public RecurringTransactionController(RecurringTransactionService recurringService)
        {
            _recurringService = recurringService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecurringTransactionRequest request)
        {
            try
            {
                var recurring = await _recurringService.CreateAsync(request);

                return Ok(new
                {
                    id = recurring.Id,
                    userId = recurring.UserId,
                    amount = recurring.Amount,
                    type = recurring.Type.ToString(),
                    category = recurring.Category,
                    description = recurring.Description,
                    frequency = recurring.Frequency.ToString(),
                    dayOfMonth = recurring.DayOfMonth,
                    nextExecutionDate = recurring.NextExecutionDate,
                    isActive = recurring.IsActive,
                    createdAt = recurring.CreatedAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(
            string userId,
            [FromQuery] string type = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var recurring = await _recurringService.GetByUserIdAsync(userGuid);
                var recurringList = recurring.ToList();

                if (!string.IsNullOrEmpty(type))
                {
                    recurringList = recurringList
                        .Where(r => r.Type.ToString().Equals(type, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (isActive.HasValue)
                {
                    recurringList = recurringList.Where(r => r.IsActive == isActive.Value).ToList();
                }

                var totalMonthlyIncome = recurringList
                    .Where(r => r.Type == Core.Domain.Enums.TransactionType.Income)
                    .Sum(r => r.Amount);

                var totalMonthlyExpenses = recurringList
                    .Where(r => r.Type == Core.Domain.Enums.TransactionType.Expense)
                    .Sum(r => r.Amount);

                return Ok(new
                {
                    data = recurringList.Select(r => new
                    {
                        id = r.Id,
                        amount = r.Amount,
                        type = r.Type.ToString(),
                        category = r.Category,
                        description = r.Description,
                        frequency = r.Frequency.ToString(),
                        dayOfMonth = r.DayOfMonth,
                        nextExecutionDate = r.NextExecutionDate,
                        isActive = r.IsActive
                    }),
                    totalMonthlyIncome,
                    totalMonthlyExpenses
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}/cashflow")]
        public async Task<IActionResult> GetCashflow(string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var cashflow = await _recurringService.GetCashflowAsync(userGuid);
                return Ok(cashflow);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] UpdateRecurringTransactionRequest request)
        {
            try
            {
                var recurringGuid = Guid.Parse(id);
                await _recurringService.UpdateAsync(
                    recurringGuid,
                    request.Amount,
                    request.Description,
                    request.EndDate);

                return Ok(new { message = "Recurring transaction updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(
            string id,
            [FromBody] ToggleRecurringTransactionRequest request)
        {
            try
            {
                var recurringGuid = Guid.Parse(id);
                await _recurringService.ToggleActiveAsync(recurringGuid, request.IsActive);

                var status = request.IsActive ? "activated" : "paused";
                return Ok(new
                {
                    id = recurringGuid,
                    isActive = request.IsActive,
                    message = $"Recurring transaction {status}"
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var recurringGuid = Guid.Parse(id);
                await _recurringService.DeleteAsync(recurringGuid);

                return Ok(new
                {
                    success = true,
                    message = "Recurring transaction deleted"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class UpdateRecurringTransactionRequest
    {
        public decimal? Amount { get; set; }
        public string Description { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ToggleRecurringTransactionRequest
    {
        public bool IsActive { get; set; }
    }
}
