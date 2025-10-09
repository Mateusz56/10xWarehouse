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

    public async Task<Supabase.Gotrue.User?> GetUserById(Guid userId)
    {
        var user = await _supabaseClient.AdminAuth(_serviceKey).GetUserById(userId.ToString());
        return user;
    }
}
