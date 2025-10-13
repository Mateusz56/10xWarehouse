using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Dtos.OrganizationDtos;

namespace _10xWarehouseNet.Services;

public interface IWarehouseService
{
    /// <summary>
    /// Gets warehouses belonging to a specific organization with pagination
    /// </summary>
    Task<(IEnumerable<Warehouse> warehouses, int totalCount)> GetOrganizationWarehousesAsync(
        Guid organizationId, string userId, int page, int pageSize);
    
    /// <summary>
    /// Gets a single warehouse by ID with its locations
    /// </summary>
    Task<Warehouse?> GetWarehouseByIdAsync(Guid warehouseId, string userId);
    
    /// <summary>
    /// Creates a new warehouse
    /// </summary>
    Task<Warehouse> CreateWarehouseAsync(CreateWarehouseRequestDto request, string userId);
    
    /// <summary>
    /// Updates an existing warehouse
    /// </summary>
    Task<Warehouse> UpdateWarehouseAsync(Guid warehouseId, UpdateWarehouseRequestDto request, string userId);
    
    /// <summary>
    /// Deletes a warehouse (only if it has no locations)
    /// </summary>
    Task DeleteWarehouseAsync(Guid warehouseId, string userId);
    
    /// <summary>
    /// Checks if a user has access to an organization
    /// </summary>
    Task<bool> UserHasAccessToOrganizationAsync(Guid organizationId, string userId);
    
    /// <summary>
    /// Checks if a user has access to a warehouse through organization membership
    /// </summary>
    Task<bool> UserHasAccessToWarehouseAsync(Guid warehouseId, string userId);
}
