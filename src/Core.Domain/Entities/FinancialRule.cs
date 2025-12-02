using System;
using Core.Domain.Common;
using Core.Domain.Enums;

namespace Core.Domain.Entities
{
    public class FinancialRule : BaseEntity
    {
        public Guid UserId { get; private set; }
        public RuleType Type { get; private set; }
        public string Category { get; private set; }
        public decimal AmountLimit { get; private set; }
        public RulePeriod Period { get; private set; }
        public bool IsActive { get; private set; }

        public User User { get; private set; }

        private FinancialRule() { }

        public FinancialRule(Guid userId, RuleType type, string category, decimal amountLimit, RulePeriod period)
        {
            UserId = userId;
            Type = type;
            Category = category;
            AmountLimit = amountLimit;
            Period = period;
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdateTimestamp();
        }
    }
}
