using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos.OrganizationDtos;

namespace _10xWarehouseNet.Services;

public interface IOrganizationService
{
    Task<Organization> CreateOrganizationAsync(CreateOrganizationRequestDto request, string userId);
    Task<(IEnumerable<Organization> organizations, int totalCount)> GetUserOrganizationsAsync(string userId, int page, int pageSize);
    Task<IEnumerable<MembershipDto>> GetUserMembershipsAsync(string userId);
}
