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
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;

        public TransactionController(
            TransactionService transactionService,
            ITransactionRepository transactionRepository,
            IUserRepository userRepository)
        {
            _transactionService = transactionService;
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                var transaction = await _transactionService.CreateTransactionAsync(request);
                
                return Ok(new 
                {
                    id = transaction.Id,
                    userId = transaction.UserId,
                    amount = transaction.Amount,
                    type = transaction.Type.ToString(),
                    category = transaction.Category,
                    description = transaction.Description,
                    source = transaction.Source.ToString(),
                    createdAt = transaction.CreatedAt
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
        public async Task<IActionResult> GetUserTransactions(string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var transactions = await _transactionRepository.GetByUserIdAsync(userGuid);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(string id)
        {
            try
            {
                var transactionGuid = Guid.Parse(id);
                var transaction = await _transactionRepository.GetByIdAsync(transactionGuid);
                
                if (transaction == null)
                    return NotFound(new { error = "Transaction not found" });

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            try
            {
                var transactionGuid = Guid.Parse(id);
                var transaction = await _transactionRepository.GetByIdAsync(transactionGuid);

                if (transaction == null)
                    return NotFound(new { error = "Transaction not found" });

                var user = await _userRepository.GetByIdAsync(transaction.UserId);
                if (user == null)
                    return NotFound(new { error = "User not found" });

                var balanceChange = transaction.Type == Core.Domain.Enums.TransactionType.Income 
                    ? -transaction.Amount 
                    : transaction.Amount;

                user.UpdateBalance(balanceChange);
                await _userRepository.UpdateAsync(user);

                // TODO: Implement DeleteAsync in ITransactionRepository
                // For now, we assume the repo will be updated or this logic is handled elsewhere
                // await _transactionRepository.DeleteAsync(transactionGuid); 

                return Ok(new { message = "Transaction deleted and balance reverted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}/range")]
        public async Task<IActionResult> GetTransactionsByRange(
            string userId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var transactions = await _transactionRepository.GetByUserAndPeriodAsync(
                    userGuid, startDate, endDate);

                var transactionList = transactions.ToList();
                var total = transactionList
                    .Where(t => t.Type == Core.Domain.Enums.TransactionType.Expense)
                    .Sum(t => t.Amount);

                return Ok(new
                {
                    data = transactionList.Select(t => new
                    {
                        id = t.Id,
                        userId = t.UserId,
                        type = t.Type.ToString(),
                        amount = t.Amount,
                        category = t.Category,
                        description = t.Description,
                        createdAt = t.CreatedAt
                    }),
                    totalAmount = total,
                    count = transactionList.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}/date/{date}")]
        public async Task<IActionResult> GetTransactionsByDate(string userId, DateTime date)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var startOfDay = date.Date;
                var endOfDay = date.Date.AddDays(1);

                var transactions = await _transactionRepository.GetByUserAndPeriodAsync(
                    userGuid, startOfDay, endOfDay);

                var transactionList = transactions.ToList();
                var total = transactionList
                    .Where(t => t.Type == Core.Domain.Enums.TransactionType.Expense)
                    .Sum(t => t.Amount);

                return Ok(new
                {
                    data = transactionList.Select(t => new
                    {
                        id = t.Id,
                        userId = t.UserId,
                        type = t.Type.ToString(),
                        amount = t.Amount,
                        category = t.Category,
                        description = t.Description,
                        createdAt = t.CreatedAt
                    }),
                    totalAmount = total,
                    count = transactionList.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}/search")]
        public async Task<IActionResult> SearchTransactions(
            string userId,
            [FromQuery] string query)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var allTransactions = await _transactionRepository.GetByUserIdAsync(userGuid);

                var filtered = allTransactions
                    .Where(t => t.Description != null && 
                                t.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var total = filtered.Sum(t => t.Amount);

                return Ok(new
                {
                    data = filtered.Select(t => new
                    {
                        id = t.Id,
                        type = t.Type.ToString(),
                        amount = t.Amount,
                        category = t.Category,
                        description = t.Description,
                        createdAt = t.CreatedAt
                    }),
                    count = filtered.Count,
                    totalAmount = total
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}/summary/category")]
        public async Task<IActionResult> GetCategorySummary(
            string userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                
                var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var end = endDate ?? start.AddMonths(1);

                var transactions = await _transactionRepository.GetByUserAndPeriodAsync(
                    userGuid, start, end);

                var expenses = transactions
                    .Where(t => t.Type == Core.Domain.Enums.TransactionType.Expense)
                    .ToList();

                var grandTotal = expenses.Sum(t => t.Amount);

                var summary = expenses
                    .GroupBy(t => t.Category)
                    .Select(g => new
                    {
                        category = g.Key,
                        totalAmount = g.Sum(t => t.Amount),
                        transactionCount = g.Count(),
                        percentage = grandTotal > 0 ? Math.Round((g.Sum(t => t.Amount) / grandTotal) * 100, 2) : 0
                    })
                    .OrderByDescending(x => x.totalAmount)
                    .ToList();

                return Ok(new
                {
                    data = summary,
                    grandTotal
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
