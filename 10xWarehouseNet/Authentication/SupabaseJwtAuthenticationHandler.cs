using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using _10xWarehouseNet.Services;

namespace _10xWarehouseNet.Authentication
{
    public class SupabaseJwtAuthenticationHandler : AuthenticationHandler<JwtBearerOptions>
    {
        private readonly SupabaseJwtAuthenticationService _authService;

        public SupabaseJwtAuthenticationHandler(
            IOptionsMonitor<JwtBearerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            SupabaseJwtAuthenticationService authService)
            : base(options, logger, encoder)
        {
            _authService = authService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.NoResult();
            }

            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return AuthenticateResult.NoResult();
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.NoResult();
            }

            try
            {
                var principal = await _authService.ValidateTokenAsync(token);
                if (principal == null)
                {
                    return AuthenticateResult.Fail("Invalid token");
                }

                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating Supabase JWT token");
                return AuthenticateResult.Fail("Token validation failed");
            }
        }
    }
}
