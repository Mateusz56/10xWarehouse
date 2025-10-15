using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Dtos.OrganizationDtos;

namespace _10xWarehouseNet.Services;

/// <summary>
/// Service interface for managing stock movements and inventory operations
/// </summary>
public interface IStockMovementService
{
    /// <summary>
    /// Gets paginated stock movements for an organization with optional filtering
    /// </summary>
    /// <param name="organizationId">The organization ID</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="productTemplateId">Optional filter by product template</param>
    /// <param name="locationId">Optional filter by location (from or to)</param>
    /// <returns>Paginated stock movements</returns>
    Task<PaginatedResponseDto<StockMovementDto>> GetStockMovementsAsync(
        Guid organizationId, 
        PaginationRequestDto pagination, 
        Guid? productTemplateId = null, 
        Guid? locationId = null);

    /// <summary>
    /// Creates a new stock movement and updates inventory levels
    /// </summary>
    /// <param name="organizationId">The organization ID</param>
    /// <param name="userId">The user ID performing the movement</param>
    /// <param name="command">The stock movement command</param>
    /// <returns>The created stock movement</returns>
    Task<StockMovementDto> CreateStockMovementAsync(
        Guid organizationId, 
        string userId, 
        CreateStockMovementCommand command);
}
