using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Application.DTOs;
using Core.Domain.Entities;

namespace Core.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress);
        Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress);
        Task<AuthResponse> RefreshTokenAsync(string refreshTokenValue, string ipAddress);
        Task RevokeTokenAsync(string refreshTokenValue, string ipAddress);
    }

    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Guid? GetUserIdFromToken(string token);
    }

    public interface ITransactionService
    {
        Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request);
    }

    public interface ISpendingValidationService
    {
        Task<SpendingValidationResponse> ValidateSpendingAsync(SpendingValidationRequest request);
    }

    public interface IRecurringTransactionService
    {
        Task<RecurringTransaction> CreateAsync(CreateRecurringTransactionRequest request);
        Task<IEnumerable<RecurringTransaction>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<RecurringTransaction>> GetActiveByUserIdAsync(Guid userId);
        Task<CashflowResponse> GetCashflowAsync(Guid userId);
        Task UpdateAsync(Guid id, decimal? amount, string description, DateTime? endDate);
        Task ToggleActiveAsync(Guid id, bool isActive);
        Task DeleteAsync(Guid id);
        Task ProcessDueTransactionsAsync();
        Task MarkAsPaidAsync(Guid id);
        Task<IEnumerable<RecurringTransaction>> GetUpcomingAsync(Guid userId, int days = 3);
    }

    public interface ITelegramService
    {
        Task<GenerateTelegramLinkResponse> GenerateLinkAsync(Guid userId);
        Task<User> LinkTelegramAsync(LinkTelegramRequest request);
        Task<TelegramStatusResponse> GetStatusAsync(Guid userId);
        Task UnlinkTelegramAsync(Guid userId);
    }
}
