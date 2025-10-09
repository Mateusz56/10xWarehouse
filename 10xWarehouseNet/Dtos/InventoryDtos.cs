using _10xWarehouseNet.Db.Enums;

namespace _10xWarehouseNet.Dtos;

// Data Transfer Objects

public record ProductSummaryDto(Guid Id, string Name);

public record LocationSummaryDto(Guid Id, string Name);

public record InventorySummaryDto(ProductSummaryDto Product, LocationSummaryDto Location, int Quantity);

public record StockMovementDto(
    Guid Id,
    Guid ProductTemplateId,
    MovementType MovementType,
    int Delta,
    Guid? FromLocationId,
    Guid? ToLocationId,
    DateTimeOffset CreatedAt
);


// Command Models

/// <summary>
/// Represents the command to create a new stock movement.
/// This single record handles all movement types.
/// For 'add', 'withdraw', and 'reconcile', use LocationId.
/// For 'move', use FromLocationId and ToLocationId.
/// The specific fields required will be validated in the handler based on MovementType.
/// </summary>
public record CreateStockMovementCommand(
    Guid ProductTemplateId,
    MovementType MovementType,
    int Delta,
    Guid? LocationId,
    Guid? FromLocationId,
    Guid? ToLocationId
);
