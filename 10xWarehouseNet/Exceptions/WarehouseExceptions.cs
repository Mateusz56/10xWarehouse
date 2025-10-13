using System.Runtime.Serialization;

namespace _10xWarehouseNet.Exceptions;

/// <summary>
/// Exception thrown when a warehouse is not found
/// </summary>
public class WarehouseNotFoundException : Exception
{
    public WarehouseNotFoundException(Guid warehouseId) 
        : base($"Warehouse with ID {warehouseId} was not found.") { }
    
    public WarehouseNotFoundException(string message) : base(message) { }
    
    public WarehouseNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    
    protected WarehouseNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when attempting to create a warehouse with a duplicate name within an organization
/// </summary>
public class DuplicateWarehouseNameException : Exception
{
    public DuplicateWarehouseNameException(string warehouseName, Guid organizationId) 
        : base($"Warehouse with name '{warehouseName}' already exists in organization {organizationId}.") { }
    
    public DuplicateWarehouseNameException(string message) : base(message) { }
    
    public DuplicateWarehouseNameException(string message, Exception innerException) : base(message, innerException) { }
    
    protected DuplicateWarehouseNameException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when attempting to delete a warehouse that contains locations
/// </summary>
public class WarehouseHasLocationsException : Exception
{
    public WarehouseHasLocationsException(Guid warehouseId) 
        : base($"Cannot delete warehouse {warehouseId} because it contains locations.") { }
    
    public WarehouseHasLocationsException(string message) : base(message) { }
    
    public WarehouseHasLocationsException(string message, Exception innerException) : base(message, innerException) { }
    
    protected WarehouseHasLocationsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when a user attempts to access a warehouse they don't have permission for
/// </summary>
public class UnauthorizedWarehouseAccessException : Exception
{
    public UnauthorizedWarehouseAccessException(Guid warehouseId, string userId) 
        : base($"User {userId} does not have access to warehouse {warehouseId}.") { }
    
    public UnauthorizedWarehouseAccessException(string message) : base(message) { }
    
    public UnauthorizedWarehouseAccessException(string message, Exception innerException) : base(message, innerException) { }
    
    protected UnauthorizedWarehouseAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
