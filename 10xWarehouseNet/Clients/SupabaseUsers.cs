using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;

namespace _10xWarehouseNet.Clients;

public class SupabaseUsers
{
    private readonly Supabase.Client _supabaseClient;
    private readonly string _serviceKey;

    public SupabaseUsers(Supabase.Client supabaseClient, string serviceKey)
    {
        _supabaseClient = supabaseClient;
        _serviceKey = serviceKey;
    }

    public async Task<Supabase.Gotrue.User?> RegisterUserAsync(string email, string password, string displayName)
    {
        try
        {
            var signUpOptions = new Supabase.Gotrue.SignUpOptions
            {
                Data = new()
                {
                    { "display_name", displayName }
                }
            };

            var user = await _supabaseClient.Auth.SignUp(email, password, signUpOptions);
            return user.User;
        }
        catch (GotrueException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to register user with Supabase: {ex.Message}", ex);
        }
    }

    public async Task<bool> ChangeUserPasswordAsync(string userId, string newPassword)
    {
        try
        {
            // Use Admin API to update user password
            var adminAuth = _supabaseClient.AdminAuth(_serviceKey);
            await adminAuth.UpdateUserById(userId, new Supabase.Gotrue.AdminUserAttributes
            {
                Password = newPassword
            });
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to change user password: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateUserDisplayNameAsync(string userId, string displayName)
    {
        try
        {
            // Use Admin API to update user metadata
            var adminAuth = _supabaseClient.AdminAuth(_serviceKey);
            await adminAuth.UpdateUserById(userId, new Supabase.Gotrue.AdminUserAttributes
            {
                UserMetadata = new Dictionary<string, object>
                {
                    { "display_name", displayName }
                }
            });
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update user display name: {ex.Message}", ex);
        }
    }

    public async Task<Supabase.Gotrue.User?> GetUserByIdAsync(string userId)
    {
        try
        {
            var adminAuth = _supabaseClient.AdminAuth(_serviceKey);
            var user = await adminAuth.GetUserById(userId);
            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get user by ID: {ex.Message}", ex);
        }
    }
}
