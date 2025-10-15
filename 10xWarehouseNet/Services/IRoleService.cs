using _10xWarehouseNet.Db.Enums;

namespace _10xWarehouseNet.Services
{
    public interface IRoleService
    {
        Task<bool> IsUserOwnerOrMemberAsync(string userId, Guid organizationId);
        Task<bool> IsUserOwnerAsync(string userId, Guid organizationId);
        Task<bool> IsUserOrganizationMemberAsync(string userId, Guid organizationId);
        Task<UserRole?> GetUserRoleAsync(string userId, Guid organizationId);
    }
}
