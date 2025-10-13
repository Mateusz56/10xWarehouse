using _10xWarehouseNet.Db;
using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace _10xWarehouseNet.Services;

public class WarehouseService : IWarehouseService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(WarehouseDbContext context, ILogger<WarehouseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Warehouse> warehouses, int totalCount)> GetOrganizationWarehousesAsync(
        Guid organizationId, string userId, int page, int pageSize)
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
                throw new UnauthorizedWarehouseAccessException($"User {userId} does not have access to organization {organizationId}.");
            }

            // Get total count for pagination
            var totalCount = await _context.Warehouses
                .Where(w => w.OrganizationId == organizationId)
                .CountAsync();

            // Get paginated warehouses
            var warehouses = await _context.Warehouses
                .Where(w => w.OrganizationId == organizationId)
                .OrderBy(w => w.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (warehouses, totalCount);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while retrieving warehouses for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("Database operation failed while retrieving warehouses.", ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving warehouses for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("Invalid database operation while retrieving warehouses.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is InvalidPaginationException || ex is UnauthorizedWarehouseAccessException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving warehouses for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("An unexpected error occurred while retrieving warehouses.", ex);
        }
    }

    public async Task<Warehouse?> GetWarehouseByIdAsync(Guid warehouseId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Get warehouse with locations
            var warehouse = await _context.Warehouses
                .Include(w => w.Locations)
                .FirstOrDefaultAsync(w => w.Id == warehouseId);

            if (warehouse == null)
            {
                return null;
            }

            // Verify user has access to the warehouse's organization
            var hasAccess = await UserHasAccessToOrganizationAsync(warehouse.OrganizationId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedWarehouseAccessException(warehouseId, userId);
            }

            return warehouse;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while retrieving warehouse {WarehouseId}", warehouseId);
            throw new DatabaseOperationException("Database operation failed while retrieving warehouse.", ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving warehouse {WarehouseId}", warehouseId);
            throw new DatabaseOperationException("Invalid database operation while retrieving warehouse.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is UnauthorizedWarehouseAccessException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving warehouse {WarehouseId}", warehouseId);
            throw new DatabaseOperationException("An unexpected error occurred while retrieving warehouse.", ex);
        }
    }

    public async Task<Warehouse> CreateWarehouseAsync(CreateWarehouseRequestDto request, string userId)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Verify user has access to the organization
            var hasAccess = await UserHasAccessToOrganizationAsync(request.OrganizationId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedWarehouseAccessException($"User {userId} does not have access to organization {request.OrganizationId}.");
            }

            // Check for duplicate warehouse name within organization
            var existingWarehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.OrganizationId == request.OrganizationId && w.Name == request.Name);

            if (existingWarehouse != null)
            {
                throw new DuplicateWarehouseNameException(request.Name, request.OrganizationId);
            }

            var warehouse = new Warehouse
            {
                Name = request.Name,
                OrganizationId = request.OrganizationId
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Warehouses.Add(warehouse);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Created warehouse {WarehouseId} with name '{WarehouseName}' for organization {OrganizationId}", 
                    warehouse.Id, warehouse.Name, warehouse.OrganizationId);

                return warehouse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating warehouse with name '{WarehouseName}' for organization {OrganizationId}", 
                    request.Name, request.OrganizationId);
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
            _logger.LogError(ex, "Database update error while creating warehouse for organization {OrganizationId}", request.OrganizationId);
            throw new DatabaseOperationException("Database operation failed while creating warehouse.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is UnauthorizedWarehouseAccessException || ex is DuplicateWarehouseNameException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while creating warehouse for organization {OrganizationId}", request.OrganizationId);
            throw new DatabaseOperationException("An unexpected error occurred while creating warehouse.", ex);
        }
    }

    public async Task<Warehouse> UpdateWarehouseAsync(Guid warehouseId, UpdateWarehouseRequestDto request, string userId)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Get existing warehouse
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == warehouseId);

            if (warehouse == null)
            {
                throw new WarehouseNotFoundException(warehouseId);
            }

            // Verify user has access to the warehouse's organization
            var hasAccess = await UserHasAccessToOrganizationAsync(warehouse.OrganizationId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedWarehouseAccessException(warehouseId, userId);
            }

            // Check for duplicate warehouse name within organization (excluding current warehouse)
            var existingWarehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.OrganizationId == warehouse.OrganizationId && 
                                        w.Name == request.Name && 
                                        w.Id != warehouseId);

            if (existingWarehouse != null)
            {
                throw new DuplicateWarehouseNameException(request.Name, warehouse.OrganizationId);
            }

            warehouse.Name = request.Name;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Warehouses.Update(warehouse);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Updated warehouse {WarehouseId} with name '{WarehouseName}'", 
                    warehouse.Id, warehouse.Name);

                return warehouse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating warehouse {WarehouseId}", warehouseId);
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
            _logger.LogError(ex, "Database update error while updating warehouse {WarehouseId}", warehouseId);
            throw new DatabaseOperationException("Database operation failed while updating warehouse.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is WarehouseNotFoundException || ex is UnauthorizedWarehouseAccessException || ex is DuplicateWarehouseNameException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while updating warehouse {WarehouseId}", warehouseId);
            throw new DatabaseOperationException("An unexpected error occurred while updating warehouse.", ex);
        }
    }

    public async Task DeleteWarehouseAsync(Guid warehouseId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Get existing warehouse
            var warehouse = await _context.Warehouses
                .Include(w => w.Locations)
                .FirstOrDefaultAsync(w => w.Id == warehouseId);

            if (warehouse == null)
            {
                throw new WarehouseNotFoundException(warehouseId);
            }

            // Verify user has access to the warehouse's organization
            var hasAccess = await UserHasAccessToOrganizationAsync(warehouse.OrganizationId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedWarehouseAccessException(warehouseId, userId);
            }

            // Check if warehouse has locations
            if (warehouse.Locations.Any())
            {
                throw new WarehouseHasLocationsException(warehouseId);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Deleted warehouse {WarehouseId} with name '{WarehouseName}'", 
                    warehouse.Id, warehouse.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting warehouse {WarehouseId}", warehouseId);
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
            _logger.LogError(ex, "Database update error while deleting warehouse {WarehouseId}", warehouseId);
            throw new DatabaseOperationException("Database operation failed while deleting warehouse.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is WarehouseNotFoundException || ex is UnauthorizedWarehouseAccessException || ex is WarehouseHasLocationsException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting warehouse {WarehouseId}", warehouseId);
            throw new DatabaseOperationException("An unexpected error occurred while deleting warehouse.", ex);
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

    public async Task<bool> UserHasAccessToWarehouseAsync(Guid warehouseId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            var hasAccess = await _context.Warehouses
                .Join(_context.OrganizationMembers,
                    w => w.OrganizationId,
                    om => om.OrganizationId,
                    (w, om) => new { Warehouse = w, Member = om })
                .AnyAsync(x => x.Warehouse.Id == warehouseId && x.Member.UserId == userGuid);

            return hasAccess;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user access to warehouse {WarehouseId} for user {UserId}", warehouseId, userId);
            return false;
        }
    }
}
