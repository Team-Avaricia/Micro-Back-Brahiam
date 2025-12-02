using System;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class TelegramLinkCode : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; }
        public DateTime? UsedAt { get; private set; }

        public User User { get; private set; } = null!;

        private TelegramLinkCode() { }

        public TelegramLinkCode(Guid userId, string code, DateTime expiresAt)
        {
            UserId = userId;
            Code = code;
            ExpiresAt = expiresAt;
            IsUsed = false;
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }
    }
}
