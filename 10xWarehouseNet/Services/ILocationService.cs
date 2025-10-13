using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Dtos.OrganizationDtos;

namespace _10xWarehouseNet.Services;

public interface ILocationService
{
    /// <summary>
    /// Gets locations belonging to a specific warehouse with pagination
    /// </summary>
    Task<(IEnumerable<Location> locations, int totalCount)> GetWarehouseLocationsAsync(
        Guid warehouseId, string userId, int page, int pageSize);
    
    /// <summary>
    /// Gets a single location by ID
    /// </summary>
    Task<Location?> GetLocationByIdAsync(Guid locationId, string userId);
    
    /// <summary>
    /// Creates a new location within a warehouse
    /// </summary>
    Task<Location> CreateLocationAsync(CreateLocationRequestDto request, string userId);
    
    /// <summary>
    /// Updates an existing location
    /// </summary>
    Task<Location> UpdateLocationAsync(Guid locationId, UpdateLocationRequestDto request, string userId);
    
    /// <summary>
    /// Deletes a location (only if it has no inventory)
    /// </summary>
    Task DeleteLocationAsync(Guid locationId, string userId);
    
    /// <summary>
    /// Checks if a user has access to a location through warehouse organization membership
    /// </summary>
    Task<bool> UserHasAccessToLocationAsync(Guid locationId, string userId);
    
    /// <summary>
    /// Checks if a user has access to a warehouse through organization membership
    /// </summary>
    Task<bool> UserHasAccessToWarehouseAsync(Guid warehouseId, string userId);
}
