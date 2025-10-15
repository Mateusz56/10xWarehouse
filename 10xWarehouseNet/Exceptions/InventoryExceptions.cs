using System.Runtime.Serialization;

namespace _10xWarehouseNet.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to access inventory they don't have permission for
/// </summary>
public class UnauthorizedInventoryAccessException : Exception
{
    public UnauthorizedInventoryAccessException(Guid organizationId, string userId) 
        : base($"User {userId} does not have access to inventory for organization {organizationId}.") { }
    
    public UnauthorizedInventoryAccessException(string message) : base(message) { }
    
    public UnauthorizedInventoryAccessException(string message, Exception innerException) : base(message, innerException) { }
    
    protected UnauthorizedInventoryAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
