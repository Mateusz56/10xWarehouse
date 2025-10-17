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

    public async Task<List<Supabase.Gotrue.User>> SearchUsersAsync(string query, int limit = 10)
    {
        try
        {
            var adminAuth = _supabaseClient.AdminAuth(_serviceKey);
            
            // Get all users and filter by query
            // Note: Supabase Admin API doesn't have built-in search, so we get all users and filter
            var users = await adminAuth.ListUsers();
            
            // Filter users by email or display name containing the query
            var filteredUsers = users.Users
                .Where(user => 
                    (user.Email?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (user.UserMetadata?.GetValueOrDefault("display_name")?.ToString()?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .Take(limit)
                .ToList();
            
            return filteredUsers;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to search users: {ex.Message}", ex);
        }
    }
}
