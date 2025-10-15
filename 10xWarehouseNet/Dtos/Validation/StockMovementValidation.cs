using _10xWarehouseNet.Db.Enums;
using System.ComponentModel.DataAnnotations;

namespace _10xWarehouseNet.Dtos.Validation;

/// <summary>
/// Validation attribute for stock movement commands
/// </summary>
public class StockMovementValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not CreateStockMovementCommand command)
        {
            return new ValidationResult("Invalid command type");
        }

        // Validate movement type specific requirements
        switch (command.MovementType)
        {
            case MovementType.Add:
            case MovementType.Withdraw:
            case MovementType.Reconcile:
                if (!command.LocationId.HasValue)
                {
                    return new ValidationResult($"{command.MovementType} operations require LocationId");
                }
                if (command.FromLocationId.HasValue || command.ToLocationId.HasValue)
                {
                    return new ValidationResult($"{command.MovementType} operations should not specify FromLocationId or ToLocationId");
                }
                if (command.Delta < 0)
                {
                    return new ValidationResult($"Delta must be non-negative for {command.MovementType} operations");
                }
                break;

            case MovementType.Move:
                if (!command.FromLocationId.HasValue || !command.ToLocationId.HasValue)
                {
                    return new ValidationResult("Move operations require both FromLocationId and ToLocationId");
                }
                if (command.LocationId.HasValue)
                {
                    return new ValidationResult("Move operations should not specify LocationId");
                }
                if (command.FromLocationId == command.ToLocationId)
                {
                    return new ValidationResult("FromLocationId and ToLocationId must be different for move operations");
                }
                if (command.Delta <= 0)
                {
                    return new ValidationResult("Delta must be positive for move operations");
                }
                break;

            default:
                return new ValidationResult($"Invalid movement type: {command.MovementType}");
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validation attribute for delta values based on movement type
/// </summary>
public class DeltaValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not int delta)
        {
            return new ValidationResult("Delta must be an integer");
        }

        // Get the movement type from the validation context
        var instance = validationContext.ObjectInstance;
        if (instance is CreateStockMovementCommand command)
        {
            switch (command.MovementType)
            {
                case MovementType.Add:
                case MovementType.Reconcile:
                    if (delta < 0)
                        return new ValidationResult($"Delta must be non-negative for {command.MovementType} operations");
                    break;

                case MovementType.Withdraw:
                case MovementType.Move:
                    if (delta <= 0)
                        return new ValidationResult($"Delta must be positive for {command.MovementType} operations");
                    break;
            }
        }

        return ValidationResult.Success;
    }
}
