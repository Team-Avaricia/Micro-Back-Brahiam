using System;
using System.Threading.Tasks;
using BCrypt.Net;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private const int RefreshTokenExpirationDays = 7;
        private const int AccessTokenExpirationMinutes = 60;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress)
        {
            await ValidateUniqueUserAsync(request.Email, request.PhoneNumber);

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User(
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.InitialBalance,
                passwordHash
            );

            await _userRepository.AddAsync(user);

            _logger.LogInformation("User registered successfully: {UserId}, Email: {Email}", user.Id, user.Email);

            return await GenerateAuthResponseAsync(user, ipAddress);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                throw new InvalidOperationException("Invalid credentials");
            }

            if (!IsPasswordValid(user.PasswordHash, request.Password))
            {
                _logger.LogWarning("Login attempt with incorrect password for: {Email}", request.Email);
                throw new InvalidOperationException("Invalid credentials");
            }

            _logger.LogInformation("User authenticated successfully: {UserId}", user.Id);

            return await GenerateAuthResponseAsync(user, ipAddress);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshTokenValue, string ipAddress)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue);
            
            if (refreshToken == null || !refreshToken.IsActive)
            {
                _logger.LogWarning("Refresh attempt with invalid or expired token");
                throw new InvalidOperationException("Invalid refresh token");
            }

            var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                _logger.LogWarning("Refresh token associated with non-existent user: {UserId}", refreshToken.UserId);
                throw new InvalidOperationException("User not found");
            }

            refreshToken.Revoke(ipAddress);
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            _logger.LogInformation("Refresh token renewed for user: {UserId}", user.Id);

            return await GenerateAuthResponseAsync(user, ipAddress);
        }

        public async Task RevokeTokenAsync(string refreshTokenValue, string ipAddress)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue);
            
            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new InvalidOperationException("Invalid refresh token");
            }

            refreshToken.Revoke(ipAddress);
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            _logger.LogInformation("Refresh token revoked for user: {UserId}", refreshToken.UserId);
        }

        private async Task ValidateUniqueUserAsync(string email, string phoneNumber)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", email);
                throw new InvalidOperationException("Email already registered");
            }

            var existingPhone = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (existingPhone != null)
            {
                _logger.LogWarning("Registration attempt with existing phone: {PhoneNumber}", phoneNumber);
                throw new InvalidOperationException("Phone number already registered");
            }
        }

        private bool IsPasswordValid(string passwordHash, string password)
        {
            return !string.IsNullOrEmpty(passwordHash) && BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user, string ipAddress)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            
            var refreshToken = new RefreshToken(
                user.Id,
                refreshTokenValue,
                DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
                ipAddress
            );

            await _refreshTokenRepository.AddAsync(refreshToken);

            return new AuthResponse
            {
                UserId = user.Id.ToString(),
                Name = user.Name,
                Email = user.Email,
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes)
            };
        }
    }
}
