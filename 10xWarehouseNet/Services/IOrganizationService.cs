using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos.OrganizationDtos;

namespace _10xWarehouseNet.Services;

public interface IOrganizationService
{
    Task<Organization> CreateOrganizationAsync(CreateOrganizationRequestDto request, string userId);
}
