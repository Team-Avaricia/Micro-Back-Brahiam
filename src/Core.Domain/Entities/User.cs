using Core.Domain.Common;
using System.Collections.Generic;

namespace Core.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public decimal CurrentBalance { get; private set; }
        public string PasswordHash { get; private set; }

        // Navigation properties
        public ICollection<Transaction> Transactions { get; private set; }
        public ICollection<FinancialRule> FinancialRules { get; private set; }
        public ICollection<RefreshToken> RefreshTokens { get; private set; }

        // Constructor privado para EF Core
        private User() 
        {
            Transactions = new List<Transaction>();
            FinancialRules = new List<FinancialRule>();
            RefreshTokens = new List<RefreshToken>();
        }

        public User(string name, string email, string phoneNumber, decimal initialBalance = 0, string passwordHash = null)
        {
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
            CurrentBalance = initialBalance;
            PasswordHash = passwordHash;
            Transactions = new List<Transaction>();
            FinancialRules = new List<FinancialRule>();
            RefreshTokens = new List<RefreshToken>();
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
    }
}
