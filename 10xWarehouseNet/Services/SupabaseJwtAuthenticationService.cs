using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Supabase;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace _10xWarehouseNet.Services
{
    public class SupabaseJwtAuthenticationService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly string _serviceKey;

        public SupabaseJwtAuthenticationService(Supabase.Client supabaseClient, string serviceKey)
        {
            _supabaseClient = supabaseClient;
            _serviceKey = serviceKey;
        }

        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                // Verify the JWT token with Supabase
                var user = await _supabaseClient.AdminAuth(_serviceKey).GetUser(token);

                if (user == null)
                    return null;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim("sub", user.Id),
                    new Claim("email", user.Email ?? ""),
                };

                // Add custom claims from user metadata
                if (user.UserMetadata != null)
                {
                    foreach (var metadata in user.UserMetadata)
                    {
                        claims.Add(new Claim($"metadata.{metadata.Key}", metadata.Value?.ToString() ?? ""));
                    }
                }

                var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                return new ClaimsPrincipal(identity);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
