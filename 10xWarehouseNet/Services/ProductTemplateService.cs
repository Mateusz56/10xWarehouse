using _10xWarehouseNet.Db;
using _10xWarehouseNet.Db.Enums;
using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace _10xWarehouseNet.Services;

public class ProductTemplateService : IProductTemplateService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<ProductTemplateService> _logger;

    public ProductTemplateService(WarehouseDbContext context, ILogger<ProductTemplateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<ProductTemplate> productTemplates, int totalCount)> GetOrganizationProductTemplatesAsync(
        Guid organizationId, string userId, int page, int pageSize, string? search = null)
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
                throw new UnauthorizedProductTemplateAccessException($"User {userId} does not have access to organization {organizationId}.");
            }

            // Build query with optional search filter
            var query = _context.ProductTemplates
                .Where(pt => pt.OrganizationId == organizationId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                query = query.Where(pt => 
                    pt.Name.ToLower().Contains(searchTerm) || 
                    (pt.Description != null && pt.Description.ToLower().Contains(searchTerm)));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Get paginated product templates
            var productTemplates = await query
                .OrderBy(pt => pt.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (productTemplates, totalCount);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is InvalidPaginationException || ex is UnauthorizedProductTemplateAccessException))
        {
            _logger.LogError(ex, "An unexpected error occurred while getting product templates for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("An unexpected error occurred while getting product templates.", ex);
        }
    }

    public async Task<ProductTemplate?> GetProductTemplateByIdAsync(Guid productTemplateId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Verify user has access to the product template
            var hasAccess = await UserHasAccessToProductTemplateAsync(productTemplateId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedProductTemplateAccessException($"User {userId} does not have access to product template {productTemplateId}.");
            }

            var productTemplate = await _context.ProductTemplates
                .FirstOrDefaultAsync(pt => pt.Id == productTemplateId);

            return productTemplate;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is UnauthorizedProductTemplateAccessException))
        {
            _logger.LogError(ex, "An unexpected error occurred while getting product template {ProductTemplateId}", productTemplateId);
            throw new DatabaseOperationException("An unexpected error occurred while getting product template.", ex);
        }
    }

    public async Task<ProductTemplate> CreateProductTemplateAsync(CreateProductTemplateRequestDto request, Guid organizationId, string userId)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Verify user has Owner role in the organization
            var isOwner = await UserIsOwnerOfOrganizationAsync(organizationId, userId);
            if (!isOwner)
            {
                throw new UnauthorizedProductTemplateAccessException($"User {userId} does not have Owner role in organization {organizationId}.");
            }

            // Check for duplicate barcode within organization (if barcode is provided)
            if (!string.IsNullOrWhiteSpace(request.Barcode))
            {
                var existingTemplate = await _context.ProductTemplates
                    .FirstOrDefaultAsync(pt => pt.OrganizationId == organizationId && pt.Barcode == request.Barcode);

                if (existingTemplate != null)
                {
                    throw new DuplicateProductTemplateBarcodeException(request.Barcode, organizationId);
                }
            }

            var productTemplate = new ProductTemplate
            {
                Name = request.Name,
                Barcode = request.Barcode,
                Description = request.Description,
                LowStockThreshold = request.LowStockThreshold,
                OrganizationId = organizationId
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.ProductTemplates.Add(productTemplate);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Created product template {ProductTemplateId} with name '{ProductTemplateName}' for organization {OrganizationId}", 
                    productTemplate.Id, productTemplate.Name, organizationId);

                return productTemplate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating product template with name '{ProductTemplateName}' for organization {OrganizationId}", 
                    request.Name, organizationId);
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while creating product template for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("Database operation failed while creating product template.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is UnauthorizedProductTemplateAccessException || ex is DuplicateProductTemplateBarcodeException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while creating product template for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("An unexpected error occurred while creating product template.", ex);
        }
    }

    public async Task DeleteProductTemplateAsync(Guid productTemplateId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Get the product template
            var productTemplate = await _context.ProductTemplates
                .FirstOrDefaultAsync(pt => pt.Id == productTemplateId);

            if (productTemplate == null)
            {
                throw new ProductTemplateNotFoundException(productTemplateId);
            }

            // Verify user has Owner role in the organization
            var isOwner = await UserIsOwnerOfOrganizationAsync(productTemplate.OrganizationId, userId);
            if (!isOwner)
            {
                throw new UnauthorizedProductTemplateAccessException($"User {userId} does not have Owner role in organization {productTemplate.OrganizationId}.");
            }

            // Check if product template has associated inventory
            var hasInventory = await _context.Inventories
                .AnyAsync(i => i.ProductTemplateId == productTemplateId);

            if (hasInventory)
            {
                throw new ProductTemplateHasInventoryException(productTemplateId);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.ProductTemplates.Remove(productTemplate);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Deleted product template {ProductTemplateId} with name '{ProductTemplateName}' from organization {OrganizationId}", 
                    productTemplate.Id, productTemplate.Name, productTemplate.OrganizationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting product template {ProductTemplateId}", productTemplateId);
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while deleting product template {ProductTemplateId}", productTemplateId);
            throw new DatabaseOperationException("Database operation failed while deleting product template.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is ProductTemplateNotFoundException || ex is UnauthorizedProductTemplateAccessException || ex is ProductTemplateHasInventoryException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting product template {ProductTemplateId}", productTemplateId);
            throw new DatabaseOperationException("An unexpected error occurred while deleting product template.", ex);
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

            var hasAccess = await _context.OrganizationMembers
                .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == userGuid);

            return hasAccess;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user access to organization {OrganizationId} for user {UserId}", organizationId, userId);
            return false;
        }
    }

    public async Task<bool> UserIsOwnerOfOrganizationAsync(Guid organizationId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            var isOwner = await _context.OrganizationMembers
                .AnyAsync(om => om.OrganizationId == organizationId && 
                               om.UserId == userGuid && 
                               om.Role == UserRole.Owner);

            return isOwner;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user owner role in organization {OrganizationId} for user {UserId}", organizationId, userId);
            return false;
        }
    }

    public async Task<bool> UserHasAccessToProductTemplateAsync(Guid productTemplateId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            var hasAccess = await _context.ProductTemplates
                .Join(_context.OrganizationMembers,
                    pt => pt.OrganizationId,
                    om => om.OrganizationId,
                    (pt, om) => new { ProductTemplate = pt, Member = om })
                .AnyAsync(x => x.ProductTemplate.Id == productTemplateId && x.Member.UserId == userGuid);

            return hasAccess;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user access to product template {ProductTemplateId} for user {UserId}", productTemplateId, userId);
            return false;
        }
    }
}
