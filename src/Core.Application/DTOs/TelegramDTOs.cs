using System;

namespace Core.Application.DTOs
{
    public class GenerateTelegramLinkResponse
    {
        public string Link { get; set; }
        public string Code { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int ExpiresInSeconds { get; set; }
    }

    public class LinkTelegramRequest
    {
        public string LinkCode { get; set; }
        public long TelegramId { get; set; }
        public string TelegramUsername { get; set; }
        public string TelegramFirstName { get; set; }
    }

    public class TelegramStatusResponse
    {
        public bool Linked { get; set; }
        public long? TelegramId { get; set; }
        public string TelegramUsername { get; set; }
        public DateTime? LinkedAt { get; set; }
        public bool PendingCode { get; set; }
        public DateTime? CodeExpiresAt { get; set; }
    }
}
