using System;
using Core.Domain.Common;
using Core.Domain.Enums;

namespace Core.Domain.Entities
{
    public class RecurringTransaction : BaseEntity
    {
        public Guid UserId { get; private set; }
        public decimal Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }
        public RecurrenceFrequency Frequency { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public int? DayOfMonth { get; private set; }
        public int? DayOfWeek { get; private set; }
        public DateTime NextExecutionDate { get; private set; }
        public bool IsActive { get; private set; }

        public User User { get; private set; }

        private RecurringTransaction() { }

        public RecurringTransaction(
            Guid userId,
            decimal amount,
            TransactionType type,
            string category,
            string description,
            RecurrenceFrequency frequency,
            DateTime startDate,
            DateTime? endDate = null,
            int? dayOfMonth = null,
            int? dayOfWeek = null)
        {
            UserId = userId;
            Amount = amount;
            Type = type;
            Category = category;
            Description = description;
            Frequency = frequency;
            StartDate = startDate;
            EndDate = endDate;
            DayOfMonth = dayOfMonth;
            DayOfWeek = dayOfWeek;
            NextExecutionDate = CalculateNextExecutionDate(startDate);
            IsActive = true;
        }

        public void UpdateNextExecutionDate()
        {
            NextExecutionDate = CalculateNextExecutionDate(NextExecutionDate);
            UpdateTimestamp();
        }

        public void Activate()
        {
            IsActive = true;
            UpdateTimestamp();
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdateTimestamp();
        }

        public void Update(
            decimal? amount = null,
            string description = null,
            DateTime? endDate = null)
        {
            if (amount.HasValue)
                Amount = amount.Value;

            if (description != null)
                Description = description;

            if (endDate.HasValue)
                EndDate = endDate.Value;

            UpdateTimestamp();
        }

        private DateTime CalculateNextExecutionDate(DateTime fromDate)
        {
            return Frequency switch
            {
                RecurrenceFrequency.Daily => fromDate.AddDays(1),
                RecurrenceFrequency.Weekly => fromDate.AddDays(7),
                RecurrenceFrequency.Monthly => fromDate.AddMonths(1),
                RecurrenceFrequency.Yearly => fromDate.AddYears(1),
                _ => fromDate.AddDays(1)
            };
        }

        public bool ShouldExecuteToday()
        {
            return IsActive && NextExecutionDate.Date <= DateTime.UtcNow.Date;
        }
    }
}
