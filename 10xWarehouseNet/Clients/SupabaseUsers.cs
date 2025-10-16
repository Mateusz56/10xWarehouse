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
}
