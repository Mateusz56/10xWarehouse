using _10xWarehouseNet.Dtos;

namespace _10xWarehouseNet.Services;

public interface IInventoryService
{
    /// <summary>
    /// Gets a paginated summary of inventory levels across all warehouses and locations within the active organization
    /// </summary>
    Task<(IEnumerable<InventorySummaryDto> inventoryItems, int totalCount)> GetInventorySummaryAsync(
        Guid organizationId, 
        string userId, 
        int page, 
        int pageSize, 
        Guid? locationId = null, 
        Guid? productTemplateId = null, 
        bool? lowStock = null);
    
    /// <summary>
    /// Checks if a user has access to an organization
    /// </summary>
    Task<bool> UserHasAccessToOrganizationAsync(Guid organizationId, string userId);
}
