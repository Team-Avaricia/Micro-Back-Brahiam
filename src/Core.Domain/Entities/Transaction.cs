using System;
using Core.Domain.Common;
using Core.Domain.Enums;

namespace Core.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public Guid UserId { get; private set; }
        public decimal Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public string Category { get; private set; }
        public DateTime Date { get; private set; }
        public TransactionSource Source { get; private set; }
        public string Description { get; private set; }

        public User User { get; private set; }

        private Transaction() { }

        public Transaction(Guid userId, decimal amount, TransactionType type, string category, DateTime date, TransactionSource source, string description)
        {
            UserId = userId;
            Amount = amount;
            Type = type;
            Category = category;
            Date = date;
            Source = source;
            Description = description;
        }
    }
}
