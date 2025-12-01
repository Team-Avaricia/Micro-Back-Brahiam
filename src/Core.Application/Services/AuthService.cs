using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Application.Services
{
    /// <summary>
    /// Servicio de autenticación
    /// </summary>
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly TokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            TokenService tokenService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress)
        {
            // Verificar si el email ya existe
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Intento de registro con email existente: {Email}", request.Email);
                throw new InvalidOperationException("El email ya está registrado");
            }

            // Verificar si el teléfono ya existe
            var existingPhone = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingPhone != null)
            {
                _logger.LogWarning("Intento de registro con teléfono existente: {PhoneNumber}", request.PhoneNumber);
                throw new InvalidOperationException("El número de teléfono ya está registrado");
            }

            // Hash de la contraseña
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Crear usuario
            var user = new User(
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.InitialBalance,
                passwordHash
            );

            await _userRepository.AddAsync(user);

            _logger.LogInformation("Usuario registrado exitosamente: {UserId}, Email: {Email}", user.Id, user.Email);

            // Generar tokens
            return await GenerateAuthResponse(user, ipAddress);
        }

        /// <summary>
        /// Inicia sesión
        /// </summary>
        public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress)
        {
            // Buscar usuario por email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Intento de login con email inexistente: {Email}", request.Email);
                throw new InvalidOperationException("Credenciales inválidas");
            }

            // Verificar contraseña
            if (string.IsNullOrEmpty(user.PasswordHash) || 
                !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Intento de login con contraseña incorrecta para: {Email}", request.Email);
                throw new InvalidOperationException("Credenciales inválidas");
            }

            _logger.LogInformation("Usuario autenticado exitosamente: {UserId}", user.Id);

            // Generar tokens
            return await GenerateAuthResponse(user, ipAddress);
        }

        /// <summary>
        /// Refresca el access token usando un refresh token
        /// </summary>
        public async Task<AuthResponse> RefreshTokenAsync(string refreshTokenValue, string ipAddress)
        {
            // Buscar refresh token
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue);
            
            if (refreshToken == null || !refreshToken.IsActive)
            {
                _logger.LogWarning("Intento de refresh con token inválido o expirado");
                throw new InvalidOperationException("Refresh token inválido");
            }

            // Obtener usuario
            var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                _logger.LogWarning("Refresh token asociado a usuario inexistente: {UserId}", refreshToken.UserId);
                throw new InvalidOperationException("Usuario no encontrado");
            }

            // Revocar el refresh token antiguo
            refreshToken.Revoke(ipAddress);
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            _logger.LogInformation("Refresh token renovado para usuario: {UserId}", user.Id);

            // Generar nuevos tokens
            return await GenerateAuthResponse(user, ipAddress);
        }

        /// <summary>
        /// Revoca un refresh token
        /// </summary>
        public async Task RevokeTokenAsync(string refreshTokenValue, string ipAddress)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenValue);
            
            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new InvalidOperationException("Refresh token inválido");
            }

            refreshToken.Revoke(ipAddress);
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            _logger.LogInformation("Refresh token revocado para usuario: {UserId}", refreshToken.UserId);
        }

        /// <summary>
        /// Genera la respuesta de autenticación con tokens
        /// </summary>
        private async Task<AuthResponse> GenerateAuthResponse(User user, string ipAddress)
        {
            // Generar Access Token (JWT)
            var accessToken = _tokenService.GenerateAccessToken(user);

            // Generar Refresh Token
            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            var refreshToken = new RefreshToken(
                user.Id,
                refreshTokenValue,
                DateTime.UtcNow.AddDays(7), // Expira en 7 días
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
                ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Mismo que el JWT
            };
        }
    }
}
