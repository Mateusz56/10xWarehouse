using _10xWarehouseNet.Db.Enums;
using _10xWarehouseNet.Dtos.Validation;
using System.ComponentModel.DataAnnotations;

namespace _10xWarehouseNet.Dtos;

// Data Transfer Objects

public record ProductSummaryDto(Guid Id, string Name);

public record LocationSummaryDto(Guid Id, string Name);

public record InventorySummaryDto(ProductSummaryDto Product, LocationSummaryDto Location, int Quantity);

public record StockMovementDto(
    Guid Id,
    Guid ProductTemplateId,
    MovementType MovementType,
    decimal Delta,
    Guid? FromLocationId,
    Guid? ToLocationId,
    DateTimeOffset CreatedAt,
    decimal Total
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
    [Required(ErrorMessage = "ProductTemplateId is required")]
    Guid ProductTemplateId,
    
    [Required(ErrorMessage = "MovementType is required")]
    MovementType MovementType,
    
    [Required(ErrorMessage = "Delta is required")]
    [DeltaValidation(ErrorMessage = "Delta validation failed")]
    int Delta,
    
    Guid? LocationId,
    Guid? FromLocationId,
    Guid? ToLocationId
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // Validate movement type specific requirements
        switch (MovementType)
        {
            case MovementType.Add:
            case MovementType.Withdraw:
            case MovementType.Reconcile:
                if (!LocationId.HasValue)
                {
                    results.Add(new ValidationResult($"{MovementType} operations require LocationId", new[] { nameof(LocationId) }));
                }
                if (FromLocationId.HasValue || ToLocationId.HasValue)
                {
                    results.Add(new ValidationResult($"{MovementType} operations should not specify FromLocationId or ToLocationId", new[] { nameof(FromLocationId), nameof(ToLocationId) }));
                }
                break;

            case MovementType.Move:
                if (!FromLocationId.HasValue || !ToLocationId.HasValue)
                {
                    results.Add(new ValidationResult("Move operations require both FromLocationId and ToLocationId", new[] { nameof(FromLocationId), nameof(ToLocationId) }));
                }
                if (LocationId.HasValue)
                {
                    results.Add(new ValidationResult("Move operations should not specify LocationId", new[] { nameof(LocationId) }));
                }
                if (FromLocationId == ToLocationId)
                {
                    results.Add(new ValidationResult("FromLocationId and ToLocationId must be different for move operations", new[] { nameof(FromLocationId), nameof(ToLocationId) }));
                }
                break;
        }

        return results;
    }
};
