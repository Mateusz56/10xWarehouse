using System.Runtime.Serialization;

namespace _10xWarehouseNet.Exceptions;

/// <summary>
/// Exception thrown when a location is not found
/// </summary>
public class LocationNotFoundException : Exception
{
    public LocationNotFoundException(Guid locationId) 
        : base($"Location with ID {locationId} was not found.") { }
    
    public LocationNotFoundException(string message) : base(message) { }
    
    public LocationNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    
    protected LocationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when attempting to create a location with a duplicate name within a warehouse
/// </summary>
public class DuplicateLocationNameException : Exception
{
    public DuplicateLocationNameException(string locationName, Guid warehouseId) 
        : base($"Location with name '{locationName}' already exists in warehouse {warehouseId}.") { }
    
    public DuplicateLocationNameException(string message) : base(message) { }
    
    public DuplicateLocationNameException(string message, Exception innerException) : base(message, innerException) { }
    
    protected DuplicateLocationNameException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when a user attempts to access a location they don't have permission for
/// </summary>
public class UnauthorizedLocationAccessException : Exception
{
    public UnauthorizedLocationAccessException(Guid locationId, string userId) 
        : base($"User {userId} does not have access to location {locationId}.") { }
    
    public UnauthorizedLocationAccessException(string message) : base(message) { }
    
    public UnauthorizedLocationAccessException(string message, Exception innerException) : base(message, innerException) { }
    
    protected UnauthorizedLocationAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when attempting to delete a location that contains inventory
/// </summary>
public class LocationHasInventoryException : Exception
{
    public LocationHasInventoryException(Guid locationId) 
        : base($"Cannot delete location {locationId} because it contains inventory.") { }
    
    public LocationHasInventoryException(string message) : base(message) { }
    
    public LocationHasInventoryException(string message, Exception innerException) : base(message, innerException) { }
    
    protected LocationHasInventoryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
