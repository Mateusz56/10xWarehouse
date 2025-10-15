using _10xWarehouseNet.Db;
using _10xWarehouseNet.Db.Enums;
using Microsoft.EntityFrameworkCore;

namespace _10xWarehouseNet.Services
{
    public class RoleService : IRoleService
    {
        private readonly WarehouseDbContext _context;

        public RoleService(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserOwnerOrMemberAsync(string userId, Guid organizationId)
        {
            var role = await GetUserRoleAsync(userId, organizationId);
            return role == UserRole.Owner || role == UserRole.Member;
        }

        public async Task<bool> IsUserOwnerAsync(string userId, Guid organizationId)
        {
            var role = await GetUserRoleAsync(userId, organizationId);
            return role == UserRole.Owner;
        }

        public async Task<bool> IsUserOrganizationMemberAsync(string userId, Guid organizationId)
        {
            var role = await GetUserRoleAsync(userId, organizationId);
            return role == UserRole.Owner || role == UserRole.Member || role == UserRole.Viewer;
        }

        public async Task<UserRole?> GetUserRoleAsync(string userId, Guid organizationId)
        {
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.UserId == Guid.Parse(userId) && om.OrganizationId == organizationId);

            return member?.Role;
        }
    }
}
