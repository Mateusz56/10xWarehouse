using _10xWarehouseNet.Db;
using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace _10xWarehouseNet.Services;

public class InventoryService : IInventoryService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(WarehouseDbContext context, ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<InventorySummaryDto> inventoryItems, int totalCount)> GetInventorySummaryAsync(
        Guid organizationId, 
        string userId, 
        int page, 
        int pageSize, 
        Guid? locationId = null, 
        Guid? productTemplateId = null, 
        bool? lowStock = null)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        if (page < 1)
        {
            throw new InvalidPaginationException("Page must be greater than 0.", nameof(page));
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new InvalidPaginationException("Page size must be between 1 and 100.", nameof(pageSize));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Verify user has access to the organization
            var hasAccess = await UserHasAccessToOrganizationAsync(organizationId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedInventoryAccessException($"User {userId} does not have access to organization {organizationId}.");
            }

            // Build the base query with joins including warehouse
            var query = _context.Inventories
                .Include(i => i.ProductTemplate)
                .Include(i => i.Location)
                .ThenInclude(l => l.Warehouse)
                .Where(i => i.OrganizationId == organizationId);

            // Apply filters
            if (locationId.HasValue)
            {
                query = query.Where(i => i.LocationId == locationId.Value);
            }

            if (productTemplateId.HasValue)
            {
                query = query.Where(i => i.ProductTemplateId == productTemplateId.Value);
            }

            if (lowStock.HasValue && lowStock.Value)
            {
                query = query.Where(i => i.Quantity <= i.ProductTemplate.LowStockThreshold);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and execute query
            var inventoryData = await query
                .OrderBy(i => i.ProductTemplate.Name)
                .ThenBy(i => i.Location.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new
                {
                    Product = new ProductSummaryDto(i.ProductTemplate.Id, i.ProductTemplate.Name),
                    Location = new LocationSummaryDto(i.Location.Id, i.Location.Name),
                    Quantity = (int)i.Quantity,
                    ProductTemplateId = i.ProductTemplateId,
                    LocationWarehouseId = i.Location.WarehouseId,
                    LowStockThreshold = i.ProductTemplate.LowStockThreshold ?? 0
                })
                .ToListAsync();

            // Calculate IsLowStock for each inventory item
            var inventoryItems = new List<InventorySummaryDto>();
            foreach (var item in inventoryData)
            {
                // Get the total quantity of this product across all locations in the same warehouse
                var totalQuantityInWarehouse = await _context.Inventories
                    .Where(i => i.ProductTemplateId == item.ProductTemplateId
                             && i.Location.WarehouseId == item.LocationWarehouseId
                             && i.OrganizationId == organizationId)
                    .SumAsync(i => i.Quantity);

                bool isLowStock = totalQuantityInWarehouse <= item.LowStockThreshold;

                inventoryItems.Add(new InventorySummaryDto(
                    item.Product,
                    item.Location,
                    item.Quantity,
                    isLowStock
                ));
            }

            return (inventoryItems, totalCount);
        }
        catch (FormatException ex) when (ex.Message.Contains("Guid"))
        {
            _logger.LogWarning(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException($"Invalid user ID format: {userId}", nameof(userId), ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is InvalidPaginationException || ex is UnauthorizedInventoryAccessException))
        {
            _logger.LogError(ex, "Database error while retrieving inventory for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException($"Database error while retrieving inventory for organization {organizationId}.", ex);
        }
    }

    public async Task<bool> UserHasAccessToOrganizationAsync(Guid organizationId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Check if user is a member of the organization
            var isMember = await _context.OrganizationMembers
                .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == userGuid);

            return isMember;
        }
        catch (FormatException ex) when (ex.Message.Contains("Guid"))
        {
            _logger.LogWarning(ex, "Invalid user ID format: {UserId}", userId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user access to organization {OrganizationId} for user {UserId}", organizationId, userId);
            return false;
        }
    }
}
