using System;

namespace Core.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = ColombiaTimeZone.Now;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = ColombiaTimeZone.Now;
        }
    }
}

