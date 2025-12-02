using System;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public string CreatedByIp { get; private set; }
        public string RevokedByIp { get; private set; }
        public DateTime? RevokedAt { get; private set; }

        public User User { get; private set; }

        private RefreshToken() { }

        public RefreshToken(Guid userId, string token, DateTime expiresAt, string createdByIp)
        {
            UserId = userId;
            Token = token;
            ExpiresAt = expiresAt;
            IsRevoked = false;
            CreatedByIp = createdByIp;
        }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        public void Revoke(string revokedByIp)
        {
            IsRevoked = true;
            RevokedByIp = revokedByIp;
            RevokedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }
    }
}
