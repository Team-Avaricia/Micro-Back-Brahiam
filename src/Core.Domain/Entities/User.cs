using Core.Domain.Common;
using System.Collections.Generic;

namespace Core.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public decimal CurrentBalance { get; private set; }
        public string? PasswordHash { get; private set; }

        public ICollection<Transaction> Transactions { get; private set; }
        public ICollection<FinancialRule> FinancialRules { get; private set; }
        public ICollection<RefreshToken> RefreshTokens { get; private set; }
        public ICollection<RecurringTransaction> RecurringTransactions { get; private set; }

        private User() 
        {
            Transactions = new List<Transaction>();
            FinancialRules = new List<FinancialRule>();
            RefreshTokens = new List<RefreshToken>();
            RecurringTransactions = new List<RecurringTransaction>();
        }

        public User(string name, string email, string phoneNumber, decimal initialBalance = 0, string? passwordHash = null)
        {
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
            CurrentBalance = initialBalance;
            PasswordHash = passwordHash;
            Transactions = new List<Transaction>();
            FinancialRules = new List<FinancialRule>();
            RefreshTokens = new List<RefreshToken>();
            RecurringTransactions = new List<RecurringTransaction>();
        }

        public void UpdateBalance(decimal amount)
        {
            CurrentBalance += amount;
            UpdateTimestamp();
        }

        public void SetPassword(string passwordHash)
        {
            PasswordHash = passwordHash;
            UpdateTimestamp();
        }

        public long? TelegramId { get; private set; }
        public string? TelegramUsername { get; private set; }

        public void LinkTelegram(long telegramId, string? username)
        {
            TelegramId = telegramId;
            TelegramUsername = username;
            UpdateTimestamp();
        }

        public void UnlinkTelegram()
        {
            TelegramId = null;
            TelegramUsername = null;
            UpdateTimestamp();
        }
    }
}
