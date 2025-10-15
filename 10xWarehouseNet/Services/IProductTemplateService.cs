using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Dtos.OrganizationDtos;

namespace _10xWarehouseNet.Services;

public interface IProductTemplateService
{
    /// <summary>
    /// Gets product templates belonging to a specific organization with pagination and search
    /// </summary>
    Task<(IEnumerable<ProductTemplate> productTemplates, int totalCount)> GetOrganizationProductTemplatesAsync(
        Guid organizationId, string userId, int page, int pageSize, string? search = null);
    
    /// <summary>
    /// Gets a single product template by ID
    /// </summary>
    Task<ProductTemplate?> GetProductTemplateByIdAsync(Guid productTemplateId, string userId);
    
    /// <summary>
    /// Creates a new product template
    /// </summary>
    Task<ProductTemplate> CreateProductTemplateAsync(CreateProductTemplateRequestDto request, Guid organizationId, string userId);
    
    /// <summary>
    /// Deletes a product template (only if it has no associated inventory)
    /// </summary>
    Task DeleteProductTemplateAsync(Guid productTemplateId, string userId);
    
    /// <summary>
    /// Checks if a user has access to an organization
    /// </summary>
    Task<bool> UserHasAccessToOrganizationAsync(Guid organizationId, string userId);
    
    /// <summary>
    /// Checks if a user has Owner role in an organization
    /// </summary>
    Task<bool> UserIsOwnerOfOrganizationAsync(Guid organizationId, string userId);
    
    /// <summary>
    /// Checks if a user has access to a product template through organization membership
    /// </summary>
    Task<bool> UserHasAccessToProductTemplateAsync(Guid productTemplateId, string userId);
}
