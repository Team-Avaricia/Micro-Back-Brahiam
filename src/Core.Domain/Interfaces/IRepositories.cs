using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<User?> GetByTelegramIdAsync(long telegramId);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
    }

    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(Guid id);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Transaction>> GetByUserAndPeriodAsync(Guid userId, DateTime start, DateTime end, string? category = null);
        Task AddAsync(Transaction transaction);
        Task DeleteAsync(Guid id);
    }

    public interface IFinancialRuleRepository
    {
        Task<FinancialRule?> GetByIdAsync(Guid id);
        Task<IEnumerable<FinancialRule>> GetActiveRulesByUserIdAsync(Guid userId);
        Task<IEnumerable<FinancialRule>> GetActiveRulesByUserAndCategoryAsync(Guid userId, string category);
        Task AddAsync(FinancialRule rule);
        Task UpdateAsync(FinancialRule rule);
        Task DeleteAsync(Guid id);
    }

    public interface IRecurringTransactionRepository
    {
        Task<RecurringTransaction?> GetByIdAsync(Guid id);
        Task<IEnumerable<RecurringTransaction>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<RecurringTransaction>> GetActiveByUserIdAsync(Guid userId);
        Task<IEnumerable<RecurringTransaction>> GetDueTransactionsAsync();
        Task AddAsync(RecurringTransaction recurringTransaction);
        Task UpdateAsync(RecurringTransaction recurringTransaction);
        Task DeleteAsync(Guid id);
    }

    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByIdAsync(Guid id);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId);
        Task AddAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task DeleteAsync(Guid id);
    }
}
