using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Application.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly IRecurringTransactionRepository _recurringRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<RecurringTransactionService> _logger;

        public RecurringTransactionService(
            IRecurringTransactionRepository recurringRepository,
            IUserRepository userRepository,
            ITransactionService transactionService,
            ILogger<RecurringTransactionService> logger)
        {
            _recurringRepository = recurringRepository;
            _userRepository = userRepository;
            _transactionService = transactionService;
            _logger = logger;
        }

        public async Task<RecurringTransaction> CreateAsync(CreateRecurringTransactionRequest request)
        {
            var userId = Guid.Parse(request.UserId);
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("Attempt to create recurring transaction for non-existent user: {UserId}", userId);
                throw new InvalidOperationException("User not found");
            }

            var recurring = new RecurringTransaction(
                userId,
                request.Amount,
                request.Type,
                request.Category,
                request.Description,
                request.Frequency,
                request.StartDate,
                request.EndDate,
                request.DayOfMonth,
                request.DayOfWeek
            );

            await _recurringRepository.AddAsync(recurring);
            
            _logger.LogInformation("Recurring transaction created: {RecurringId} for user {UserId}, Amount: {Amount}, Frequency: {Frequency}",
                recurring.Id, userId, request.Amount, request.Frequency);
            
            return recurring;
        }

        public async Task<IEnumerable<RecurringTransaction>> GetByUserIdAsync(Guid userId)
        {
            return await _recurringRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<RecurringTransaction>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _recurringRepository.GetActiveByUserIdAsync(userId);
        }

        public async Task<CashflowResponse> GetCashflowAsync(Guid userId)
        {
            var activeRecurring = await _recurringRepository.GetActiveByUserIdAsync(userId);
            var recurringList = activeRecurring.ToList();

            var incomes = recurringList.Where(r => r.Type == TransactionType.Income).ToList();
            var expenses = recurringList.Where(r => r.Type == TransactionType.Expense).ToList();

            var totalMonthlyIncome = incomes
                .Where(r => r.Frequency == RecurrenceFrequency.Monthly)
                .Sum(r => r.Amount);

            var totalMonthlyExpenses = expenses
                .Where(r => r.Frequency == RecurrenceFrequency.Monthly)
                .Sum(r => r.Amount);

            return new CashflowResponse
            {
                TotalMonthlyIncome = totalMonthlyIncome,
                TotalMonthlyExpenses = totalMonthlyExpenses,
                NetMonthlyCashflow = totalMonthlyIncome - totalMonthlyExpenses,
                IncomeBreakdown = incomes
                    .GroupBy(r => r.Category)
                    .Select(g => new CashflowItem
                    {
                        Category = g.Key,
                        Amount = g.Sum(r => r.Amount)
                    }),
                ExpenseBreakdown = expenses
                    .GroupBy(r => r.Category)
                    .Select(g => new CashflowItem
                    {
                        Category = g.Key,
                        Amount = g.Sum(r => r.Amount)
                    })
            };
        }

        public async Task UpdateAsync(Guid id, decimal? amount, string description, DateTime? endDate)
        {
            var recurring = await _recurringRepository.GetByIdAsync(id);
            if (recurring == null)
            {
                _logger.LogWarning("Attempt to update non-existent recurring transaction: {RecurringId}", id);
                throw new InvalidOperationException("Recurring transaction not found");
            }

            recurring.Update(amount, description, endDate);
            await _recurringRepository.UpdateAsync(recurring);
            
            _logger.LogInformation("Recurring transaction updated: {RecurringId}", id);
        }

        public async Task ToggleActiveAsync(Guid id, bool isActive)
        {
            var recurring = await _recurringRepository.GetByIdAsync(id);
            if (recurring == null)
            {
                _logger.LogWarning("Attempt to toggle non-existent recurring transaction: {RecurringId}", id);
                throw new InvalidOperationException("Recurring transaction not found");
            }

            if (isActive)
                recurring.Activate();
            else
                recurring.Deactivate();

            await _recurringRepository.UpdateAsync(recurring);
            
            _logger.LogInformation("Recurring transaction {Status}: {RecurringId}", 
                isActive ? "activated" : "paused", id);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _recurringRepository.DeleteAsync(id);
            _logger.LogInformation("Recurring transaction deleted: {RecurringId}", id);
        }

        public async Task ProcessDueTransactionsAsync()
        {
            _logger.LogInformation("Starting processing of recurring transactions");
            
            var dueTransactions = await _recurringRepository.GetDueTransactionsAsync();
            var dueList = dueTransactions.ToList();
            
            _logger.LogInformation("Found {Count} recurring transactions to process", dueList.Count);

            int processed = 0;
            int failed = 0;

            foreach (var recurring in dueList)
            {
                try
                {
                    await _transactionService.CreateTransactionAsync(new CreateTransactionRequest
                    {
                        UserId = recurring.UserId.ToString(),
                        Amount = recurring.Amount,
                        Type = recurring.Type,
                        Category = recurring.Category,
                        Description = $"{recurring.Description} (Recurring)",
                        Source = TransactionSource.Automatic
                    });

                    recurring.UpdateNextExecutionDate();
                    await _recurringRepository.UpdateAsync(recurring);
                    
                    processed++;
                    
                    _logger.LogInformation("Recurring transaction processed: {RecurringId}, User: {UserId}, Amount: {Amount}",
                        recurring.Id, recurring.UserId, recurring.Amount);
                }
                catch (Exception ex)
                {
                    failed++;
                    _logger.LogError(ex, "Error processing recurring transaction: {RecurringId}", recurring.Id);
                    continue;
                }
            }
            
            _logger.LogInformation("Processing completed. Successful: {Processed}, Failed: {Failed}", 
                processed, failed);
        }

        public async Task MarkAsPaidAsync(Guid id)
        {
            var recurring = await _recurringRepository.GetByIdAsync(id);
            if (recurring == null)
            {
                _logger.LogWarning("Attempt to mark as paid non-existent recurring transaction: {RecurringId}", id);
                throw new InvalidOperationException("Recurring transaction not found");
            }

            recurring.MarkAsPaid();
            await _recurringRepository.UpdateAsync(recurring);
            
            _logger.LogInformation("Recurring transaction marked as paid: {RecurringId}", id);
        }

        public async Task<IEnumerable<RecurringTransaction>> GetUpcomingAsync(Guid userId, int days = 3)
        {
            var recurring = await _recurringRepository.GetByUserIdAsync(userId);
            return recurring.Where(r => r.IsDueWithinDays(days)).OrderBy(r => r.NextExecutionDate);
        }
    }
}
