using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace API.Authentication
{
    /// <summary>
    /// Custom authentication handler that allows internal microservices to bypass JWT
    /// using an API Key passed in the X-Api-Key header.
    /// </summary>
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly IConfiguration _configuration;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if X-Api-Key header is present
            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            if (string.IsNullOrEmpty(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Get the valid API key from configuration
            var validApiKey = _configuration["InternalApi:ApiKey"];
            if (string.IsNullOrEmpty(validApiKey))
            {
                Logger.LogWarning("Internal API Key is not configured");
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Validate the API key
            if (!string.Equals(providedApiKey, validApiKey, StringComparison.Ordinal))
            {
                Logger.LogWarning("Invalid API Key provided");
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }

            // Create claims for the service account
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "InternalService"),
                new Claim(ClaimTypes.Role, "Service"),
                new Claim("service_type", "microservice")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Logger.LogInformation("🔑 Internal service authenticated via API Key");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
