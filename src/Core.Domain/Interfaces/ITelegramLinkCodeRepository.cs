using System;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Domain.Interfaces
{
    public interface ITelegramLinkCodeRepository
    {
        Task<TelegramLinkCode?> GetByCodeAsync(string code);
        Task<TelegramLinkCode?> GetPendingCodeByUserIdAsync(Guid userId);
        Task AddAsync(TelegramLinkCode telegramLinkCode);
        Task UpdateAsync(TelegramLinkCode telegramLinkCode);
        Task RemoveRangeAsync(System.Collections.Generic.IEnumerable<TelegramLinkCode> codes);
        Task<System.Collections.Generic.IEnumerable<TelegramLinkCode>> GetExpiredCodesAsync();
    }
}
