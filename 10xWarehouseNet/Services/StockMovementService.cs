using _10xWarehouseNet.Db;
using _10xWarehouseNet.Db.Enums;
using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Dtos.OrganizationDtos;
using Microsoft.EntityFrameworkCore;

namespace _10xWarehouseNet.Services;

/// <summary>
/// Service implementation for managing stock movements and inventory operations
/// </summary>
public class StockMovementService : IStockMovementService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<StockMovementService> _logger;

    public StockMovementService(WarehouseDbContext context, ILogger<StockMovementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets paginated stock movements for an organization with optional filtering
    /// </summary>
    public async Task<PaginatedResponseDto<StockMovementDto>> GetStockMovementsAsync(
        Guid organizationId, 
        PaginationRequestDto pagination, 
        Guid? productTemplateId = null, 
        Guid? locationId = null)
    {
        try
        {
            var query = _context.StockMovements
                .Where(sm => sm.OrganizationId == organizationId);

            // Apply filters
            if (productTemplateId.HasValue)
            {
                query = query.Where(sm => sm.ProductTemplateId == productTemplateId.Value);
            }

            if (locationId.HasValue)
            {
                query = query.Where(sm => sm.FromLocationId == locationId.Value || sm.ToLocationId == locationId.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var stockMovements = await query
                .OrderByDescending(sm => sm.CreatedAt)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(sm => new StockMovementDto(
                    sm.Id,
                    sm.ProductTemplateId,
                    sm.MovementType,
                    sm.Delta,
                    sm.FromLocationId,
                    sm.ToLocationId,
                    sm.CreatedAt,
                    sm.Total
                ))
                .ToListAsync();

            var paginationDto = new PaginationDto(pagination.Page, pagination.PageSize, totalCount);
            return new PaginatedResponseDto<StockMovementDto>(stockMovements, paginationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock movements for organization {OrganizationId}", organizationId);
            throw;
        }
    }

    /// <summary>
    /// Creates a new stock movement and updates inventory levels
    /// </summary>
    public async Task<StockMovementDto> CreateStockMovementAsync(
        Guid organizationId, 
        string userId, 
        CreateStockMovementCommand command)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Validate business rules
            await ValidateStockMovementAsync(organizationId, command);

            // Create stock movement record
            var deltaValue = command.MovementType switch
            {
                MovementType.Withdraw => -command.Delta,
                MovementType.Reconcile => command.Delta - await GetCurrentInventoryAsync(organizationId, command.ProductTemplateId, command.LocationId!.Value),
                _ => command.Delta
            };
            _logger.LogInformation("Creating stock movement: Type={MovementType}, CommandDelta={CommandDelta}, SavedDelta={SavedDelta}", 
                command.MovementType, command.Delta, deltaValue);
                
            var stockMovement = new StockMovement
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ProductTemplateId = command.ProductTemplateId,
                MovementType = command.MovementType,
                Delta = deltaValue,
                FromLocationId = command.MovementType switch
                {
                    MovementType.Move => command.FromLocationId,
                    MovementType.Withdraw or MovementType.Reconcile => command.LocationId,
                    _ => null
                },
                ToLocationId = command.MovementType switch
                {
                    MovementType.Move => command.ToLocationId,
                    MovementType.Add or MovementType.Reconcile => command.LocationId,
                    _ => null
                },
                UserId = Guid.Parse(userId),
                CreatedAt = DateTimeOffset.UtcNow
            };

            // For Move operations, get current quantities before updating inventory
            if (command.MovementType == MovementType.Move)
            {
                // Get current quantities before the move
                var sourceCurrentQuantity = await GetCurrentInventoryAsync(organizationId, command.ProductTemplateId, command.FromLocationId!.Value);
                var destinationCurrentQuantity = await GetCurrentInventoryAsync(organizationId, command.ProductTemplateId, command.ToLocationId!.Value);

                // Create MoveSubtract record for source location
                var moveSubtract = new StockMovement
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    ProductTemplateId = command.ProductTemplateId,
                    MovementType = MovementType.MoveSubtract,
                    Delta = -command.Delta, // Negative delta for subtraction
                    FromLocationId = command.FromLocationId,
                    ToLocationId = command.ToLocationId,
                    UserId = Guid.Parse(userId),
                    CreatedAt = DateTimeOffset.UtcNow,
                    Total = sourceCurrentQuantity - command.Delta // Quantity after subtraction
                };

                // Create MoveAdd record for destination location
                var moveAdd = new StockMovement
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    ProductTemplateId = command.ProductTemplateId,
                    MovementType = MovementType.MoveAdd,
                    Delta = command.Delta, // Positive delta for addition
                    FromLocationId = command.FromLocationId,
                    ToLocationId = command.ToLocationId,
                    UserId = Guid.Parse(userId),
                    CreatedAt = DateTimeOffset.UtcNow,
                    Total = destinationCurrentQuantity + command.Delta // Quantity after addition
                };

                _context.StockMovements.AddRange(moveSubtract, moveAdd);
                
                // Update inventory after creating stock movement records
                await UpdateInventoryAsync(organizationId, command);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Move operation created: MoveSubtract and MoveAdd for product {ProductId} in organization {OrganizationId}", 
                    command.ProductTemplateId, organizationId);

                // Return the MoveAdd record as the primary result
                return new StockMovementDto(
                    moveAdd.Id,
                    moveAdd.ProductTemplateId,
                    moveAdd.MovementType,
                    moveAdd.Delta,
                    moveAdd.FromLocationId,
                    moveAdd.ToLocationId,
                    moveAdd.CreatedAt,
                    moveAdd.Total
                );
            }
            else
            {
                // For non-Move operations, create single record
                var targetLocationId = command.MovementType switch
                {
                    MovementType.Add or MovementType.Withdraw or MovementType.Reconcile => command.LocationId!.Value,
                    _ => throw new InvalidOperationException($"Unsupported movement type: {command.MovementType}")
                };
                
                // Get current quantity before updating inventory
                var currentQuantity = await GetCurrentInventoryAsync(organizationId, command.ProductTemplateId, targetLocationId);
                stockMovement.Total = command.MovementType switch
                {
                    MovementType.Add => currentQuantity + command.Delta,
                    MovementType.Withdraw => currentQuantity + stockMovement.Delta, // stockMovement.Delta is already negative
                    MovementType.Reconcile => command.Delta, // For reconcile, total is the new quantity (command.Delta)
                    _ => currentQuantity
                };

                _context.StockMovements.Add(stockMovement);
                
                // Update inventory after creating stock movement record
                await UpdateInventoryAsync(organizationId, command);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Stock movement created: {MovementType} for product {ProductId} in organization {OrganizationId}", 
                    command.MovementType, command.ProductTemplateId, organizationId);

                return new StockMovementDto(
                    stockMovement.Id,
                    stockMovement.ProductTemplateId,
                    stockMovement.MovementType,
                    stockMovement.Delta,
                    stockMovement.FromLocationId,
                    stockMovement.ToLocationId,
                    stockMovement.CreatedAt,
                    stockMovement.Total
                );
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating stock movement for organization {OrganizationId}", organizationId);
            throw;
        }
    }

    /// <summary>
    /// Validates business rules for stock movement
    /// </summary>
    private async Task ValidateStockMovementAsync(Guid organizationId, CreateStockMovementCommand command)
    {
        // Validate product template exists in organization
        var productExists = await _context.ProductTemplates
            .AnyAsync(pt => pt.Id == command.ProductTemplateId && pt.OrganizationId == organizationId);
        
        if (!productExists)
        {
            throw new InvalidOperationException($"Product template {command.ProductTemplateId} not found in organization {organizationId}");
        }

        // Validate locations exist in organization
        if (command.LocationId.HasValue)
        {
            var locationExists = await _context.Locations
                .AnyAsync(l => l.Id == command.LocationId.Value && l.Warehouse.OrganizationId == organizationId);
            
            if (!locationExists)
            {
                throw new InvalidOperationException($"Location {command.LocationId} not found in organization {organizationId}");
            }
        }

        if (command.FromLocationId.HasValue)
        {
            var fromLocationExists = await _context.Locations
                .AnyAsync(l => l.Id == command.FromLocationId.Value && l.Warehouse.OrganizationId == organizationId);
            
            if (!fromLocationExists)
            {
                throw new InvalidOperationException($"From location {command.FromLocationId} not found in organization {organizationId}");
            }
        }

        if (command.ToLocationId.HasValue)
        {
            var toLocationExists = await _context.Locations
                .AnyAsync(l => l.Id == command.ToLocationId.Value && l.Warehouse.OrganizationId == organizationId);
            
            if (!toLocationExists)
            {
                throw new InvalidOperationException($"To location {command.ToLocationId} not found in organization {organizationId}");
            }
        }

        // Validate movement type specific rules
        switch (command.MovementType)
        {
            case MovementType.Add:
            case MovementType.Reconcile:
                if (!command.LocationId.HasValue)
                    throw new InvalidOperationException("LocationId is required for add/reconcile operations");
                if (command.Delta < 0)
                    throw new InvalidOperationException("Delta must be non-negative for add/reconcile operations");
                break;

            case MovementType.Withdraw:
                if (!command.LocationId.HasValue)
                    throw new InvalidOperationException("LocationId is required for withdraw operations");
                if (command.Delta <= 0)
                    throw new InvalidOperationException("Delta must be positive for withdraw operations");
                
                // Check sufficient inventory
                var currentQuantity = await GetCurrentInventoryAsync(organizationId, command.ProductTemplateId, command.LocationId.Value);
                if (currentQuantity < command.Delta)
                {
                    throw new InvalidOperationException($"Insufficient inventory. Current: {currentQuantity}, Requested: {command.Delta}");
                }
                break;

            case MovementType.Move:
                if (!command.FromLocationId.HasValue || !command.ToLocationId.HasValue)
                    throw new InvalidOperationException("FromLocationId and ToLocationId are required for move operations");
                if (command.FromLocationId == command.ToLocationId)
                    throw new InvalidOperationException("FromLocationId and ToLocationId must be different for move operations");
                if (command.Delta <= 0)
                    throw new InvalidOperationException("Delta must be positive for move operations");
                
                // Check sufficient inventory at source
                var sourceQuantity = await GetCurrentInventoryAsync(organizationId, command.ProductTemplateId, command.FromLocationId.Value);
                if (sourceQuantity < command.Delta)
                {
                    throw new InvalidOperationException($"Insufficient inventory at source location. Current: {sourceQuantity}, Requested: {command.Delta}");
                }
                break;
        }
    }

    /// <summary>
    /// Updates inventory based on movement type
    /// </summary>
    private async Task UpdateInventoryAsync(Guid organizationId, CreateStockMovementCommand command)
    {
        switch (command.MovementType)
        {
            case MovementType.Add:
                await AddToInventoryAsync(organizationId, command.ProductTemplateId, command.LocationId!.Value, command.Delta);
                break;

            case MovementType.Withdraw:
                await SubtractFromInventoryAsync(organizationId, command.ProductTemplateId, command.LocationId!.Value, command.Delta);
                break;

            case MovementType.Move:
                await SubtractFromInventoryAsync(organizationId, command.ProductTemplateId, command.FromLocationId!.Value, command.Delta);
                await AddToInventoryAsync(organizationId, command.ProductTemplateId, command.ToLocationId!.Value, command.Delta);
                break;

            case MovementType.Reconcile:
                await SetInventoryQuantityAsync(organizationId, command.ProductTemplateId, command.LocationId!.Value, command.Delta);
                break;
        }
    }

    /// <summary>
    /// Adds quantity to inventory
    /// </summary>
    private async Task AddToInventoryAsync(Guid organizationId, Guid productTemplateId, Guid locationId, int delta)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.OrganizationId == organizationId && 
                                     i.ProductTemplateId == productTemplateId && 
                                     i.LocationId == locationId);

        if (inventory == null)
        {
            inventory = new Inventory
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ProductTemplateId = productTemplateId,
                LocationId = locationId,
                Quantity = delta
            };
            _context.Inventories.Add(inventory);
        }
        else
        {
            inventory.Quantity += delta;
        }
    }

    /// <summary>
    /// Subtracts quantity from inventory
    /// </summary>
    private async Task SubtractFromInventoryAsync(Guid organizationId, Guid productTemplateId, Guid locationId, int delta)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.OrganizationId == organizationId && 
                                     i.ProductTemplateId == productTemplateId && 
                                     i.LocationId == locationId);

        if (inventory == null)
        {
            throw new InvalidOperationException($"No inventory found for product {productTemplateId} at location {locationId}");
        }

        inventory.Quantity -= delta;
        
        if (inventory.Quantity < 0)
        {
            throw new InvalidOperationException($"Inventory would become negative. Current: {inventory.Quantity + delta}, Requested: {delta}");
        }
    }

    /// <summary>
    /// Sets inventory quantity to a specific value
    /// </summary>
    private async Task SetInventoryQuantityAsync(Guid organizationId, Guid productTemplateId, Guid locationId, int quantity)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.OrganizationId == organizationId && 
                                     i.ProductTemplateId == productTemplateId && 
                                     i.LocationId == locationId);

        if (inventory == null)
        {
            inventory = new Inventory
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ProductTemplateId = productTemplateId,
                LocationId = locationId,
                Quantity = quantity
            };
            _context.Inventories.Add(inventory);
        }
        else
        {
            inventory.Quantity = quantity;
        }
    }

    /// <summary>
    /// Gets current inventory quantity
    /// </summary>
    private async Task<decimal> GetCurrentInventoryAsync(Guid organizationId, Guid productTemplateId, Guid locationId)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.OrganizationId == organizationId && 
                                     i.ProductTemplateId == productTemplateId && 
                                     i.LocationId == locationId);

        return inventory?.Quantity ?? 0;
    }

}
