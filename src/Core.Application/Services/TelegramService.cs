using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Application.Services
{
    public class TelegramService
    {
        private readonly ITelegramLinkCodeRepository _linkCodeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelegramService> _logger;

        public TelegramService(
            ITelegramLinkCodeRepository linkCodeRepository,
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<TelegramService> logger)
        {
            _linkCodeRepository = linkCodeRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GenerateTelegramLinkResponse> GenerateLinkAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.TelegramId.HasValue)
                throw new InvalidOperationException("User already has Telegram linked");

            // Invalidate old codes (optional, but good practice to clean up if we had a method)
            // For now, we just generate a new one.

            var code = GenerateUniqueCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(15);

            var linkCode = new TelegramLinkCode(userId, code, expiresAt);
            await _linkCodeRepository.AddAsync(linkCode);

            var botUsername = _configuration["Telegram:BotUsername"] ?? "AvariciaBot";

            return new GenerateTelegramLinkResponse
            {
                Link = $"https://t.me/{botUsername}?start=LINK_{code}",
                Code = code,
                ExpiresAt = expiresAt,
                ExpiresInSeconds = 900
            };
        }

        public async Task<User> LinkTelegramAsync(LinkTelegramRequest request)
        {
            // 1. Check if Telegram ID is already linked to another user
            var existingUser = await _userRepository.GetByTelegramIdAsync(request.TelegramId);
            if (existingUser != null)
                throw new InvalidOperationException("This Telegram account is already linked to another user");

            // 2. Validate code
            var linkCode = await _linkCodeRepository.GetByCodeAsync(request.LinkCode);
            if (linkCode == null)
                throw new InvalidOperationException("Invalid link code");

            if (linkCode.IsUsed)
                throw new InvalidOperationException("Link code already used");

            if (linkCode.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Link code expired");

            // 3. Link user
            var user = linkCode.User;
            user.LinkTelegram(request.TelegramId, request.TelegramUsername);
            linkCode.MarkAsUsed();

            await _userRepository.UpdateAsync(user);
            await _linkCodeRepository.UpdateAsync(linkCode);

            _logger.LogInformation("Telegram {TelegramId} linked to user {UserId}", request.TelegramId, user.Id);

            return user;
        }

        public async Task<TelegramStatusResponse> GetStatusAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.TelegramId.HasValue)
            {
                return new TelegramStatusResponse
                {
                    Linked = true,
                    TelegramId = user.TelegramId,
                    TelegramUsername = user.TelegramUsername,
                    LinkedAt = user.UpdatedAt
                };
            }

            var pendingCode = await _linkCodeRepository.GetPendingCodeByUserIdAsync(userId);

            return new TelegramStatusResponse
            {
                Linked = false,
                PendingCode = pendingCode != null,
                CodeExpiresAt = pendingCode?.ExpiresAt
            };
        }

        public async Task UnlinkTelegramAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (!user.TelegramId.HasValue)
                throw new InvalidOperationException("No Telegram account linked");

            user.UnlinkTelegram();
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("Telegram account unlinked for user {UserId}", userId);
        }

        private string GenerateUniqueCode()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
