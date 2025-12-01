using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> GetByIdAsync(Guid id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByUserAndPeriodAsync(Guid userId, DateTime start, DateTime end, string category = null)
        {
            var query = _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= start && t.Date < end);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category == category);
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class FinancialRuleRepository : IFinancialRuleRepository
    {
        private readonly ApplicationDbContext _context;

        public FinancialRuleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FinancialRule> GetByIdAsync(Guid id)
        {
            return await _context.FinancialRules.FindAsync(id);
        }

        public async Task<IEnumerable<FinancialRule>> GetActiveRulesByUserIdAsync(Guid userId)
        {
            return await _context.FinancialRules
                .Where(r => r.UserId == userId && r.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancialRule>> GetActiveRulesByUserAndCategoryAsync(Guid userId, string category)
        {
            return await _context.FinancialRules
                .Where(r => r.UserId == userId && r.IsActive && r.Category == category)
                .ToListAsync();
        }

        public async Task AddAsync(FinancialRule rule)
        {
            await _context.FinancialRules.AddAsync(rule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FinancialRule rule)
        {
            _context.FinancialRules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var rule = await _context.FinancialRules.FindAsync(id);
            if (rule != null)
            {
                _context.FinancialRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class RecurringTransactionRepository : IRecurringTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public RecurringTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RecurringTransaction> GetByIdAsync(Guid id)
        {
            return await _context.RecurringTransactions.FindAsync(id);
        }

        public async Task<IEnumerable<RecurringTransaction>> GetByUserIdAsync(Guid userId)
        {
            return await _context.RecurringTransactions
                .Where(rt => rt.UserId == userId)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RecurringTransaction>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.RecurringTransactions
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<RecurringTransaction>> GetDueTransactionsAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.RecurringTransactions
                .Where(rt => rt.IsActive && rt.NextExecutionDate.Date <= today)
                .ToListAsync();
        }

        public async Task AddAsync(RecurringTransaction recurringTransaction)
        {
            await _context.RecurringTransactions.AddAsync(recurringTransaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RecurringTransaction recurringTransaction)
        {
            _context.RecurringTransactions.Update(recurringTransaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var recurringTransaction = await _context.RecurringTransactions.FindAsync(id);
            if (recurringTransaction != null)
            {
                _context.RecurringTransactions.Remove(recurringTransaction);
                await _context.SaveChangesAsync();
            }
        }
    }
}
