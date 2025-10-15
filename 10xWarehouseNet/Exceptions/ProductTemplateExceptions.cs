using System.Runtime.Serialization;

namespace _10xWarehouseNet.Exceptions;

/// <summary>
/// Exception thrown when a product template is not found
/// </summary>
public class ProductTemplateNotFoundException : Exception
{
    public ProductTemplateNotFoundException(Guid productTemplateId) 
        : base($"Product template with ID {productTemplateId} was not found.") { }
    
    public ProductTemplateNotFoundException(string message) : base(message) { }
    
    public ProductTemplateNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    
    protected ProductTemplateNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when attempting to create a product template with a duplicate barcode within an organization
/// </summary>
public class DuplicateProductTemplateBarcodeException : Exception
{
    public DuplicateProductTemplateBarcodeException(string barcode, Guid organizationId) 
        : base($"Product template with barcode '{barcode}' already exists in organization {organizationId}.") { }
    
    public DuplicateProductTemplateBarcodeException(string message) : base(message) { }
    
    public DuplicateProductTemplateBarcodeException(string message, Exception innerException) : base(message, innerException) { }
    
    protected DuplicateProductTemplateBarcodeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when a user attempts to access a product template they don't have permission for
/// </summary>
public class UnauthorizedProductTemplateAccessException : Exception
{
    public UnauthorizedProductTemplateAccessException(Guid productTemplateId, string userId) 
        : base($"User {userId} does not have access to product template {productTemplateId}.") { }
    
    public UnauthorizedProductTemplateAccessException(string message) : base(message) { }
    
    public UnauthorizedProductTemplateAccessException(string message, Exception innerException) : base(message, innerException) { }
    
    protected UnauthorizedProductTemplateAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Exception thrown when attempting to delete a product template that has associated inventory
/// </summary>
public class ProductTemplateHasInventoryException : Exception
{
    public ProductTemplateHasInventoryException(Guid productTemplateId) 
        : base($"Cannot delete product template {productTemplateId} because it has associated inventory.") { }
    
    public ProductTemplateHasInventoryException(string message) : base(message) { }
    
    public ProductTemplateHasInventoryException(string message, Exception innerException) : base(message, innerException) { }
    
    protected ProductTemplateHasInventoryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
