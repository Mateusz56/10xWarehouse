using _10xWarehouseNet.Db;
using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace _10xWarehouseNet.Services;

public class LocationService : ILocationService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<LocationService> _logger;

    public LocationService(WarehouseDbContext context, ILogger<LocationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Location> locations, int totalCount)> GetWarehouseLocationsAsync(
        Guid warehouseId, string userId, int page, int pageSize)
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

            // Verify user has access to the warehouse
            var hasAccess = await UserHasAccessToWarehouseAsync(warehouseId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedLocationAccessException($"User {userId} does not have access to warehouse {warehouseId}.");
            }

            // Verify warehouse exists
            var warehouseExists = await _context.Warehouses
                .AnyAsync(w => w.Id == warehouseId);
            if (!warehouseExists)
            {
                throw new WarehouseNotFoundException(warehouseId);
            }

            // Get total count for pagination
            var totalCount = await _context.Locations
                .Where(l => l.WarehouseId == warehouseId)
                .CountAsync();

            // Get paginated locations
            var locations = await _context.Locations
                .Where(l => l.WarehouseId == warehouseId)
                .OrderBy(l => l.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (locations, totalCount);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while retrieving locations for warehouse {WarehouseId}", warehouseId);
            throw new DatabaseOperationException("Database operation failed while retrieving locations.", ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving locations for warehouse {WarehouseId}", warehouseId);
            throw new DatabaseOperationException("Invalid database operation while retrieving locations.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is InvalidPaginationException || ex is UnauthorizedLocationAccessException || ex is WarehouseNotFoundException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving locations for warehouse {WarehouseId}", warehouseId);
            throw new DatabaseOperationException("An unexpected error occurred while retrieving locations.", ex);
        }
    }

    public async Task<Location?> GetLocationByIdAsync(Guid locationId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Get location
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == locationId);

            if (location == null)
            {
                return null;
            }

            // Verify user has access to the location's warehouse
            var hasAccess = await UserHasAccessToWarehouseAsync(location.WarehouseId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedLocationAccessException(locationId, userId);
            }

            return location;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while retrieving location {LocationId}", locationId);
            throw new DatabaseOperationException("Database operation failed while retrieving location.", ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving location {LocationId}", locationId);
            throw new DatabaseOperationException("Invalid database operation while retrieving location.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is UnauthorizedLocationAccessException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving location {LocationId}", locationId);
            throw new DatabaseOperationException("An unexpected error occurred while retrieving location.", ex);
        }
    }

    public async Task<Location> CreateLocationAsync(CreateLocationRequestDto request, string userId)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Verify user has access to the warehouse
            var hasAccess = await UserHasAccessToWarehouseAsync(request.WarehouseId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedLocationAccessException($"User {userId} does not have access to warehouse {request.WarehouseId}.");
            }

            // Verify warehouse exists
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == request.WarehouseId);
            if (warehouse == null)
            {
                throw new WarehouseNotFoundException(request.WarehouseId);
            }

            // Check for duplicate location name within warehouse
            var existingLocation = await _context.Locations
                .FirstOrDefaultAsync(l => l.WarehouseId == request.WarehouseId && l.Name == request.Name);

            if (existingLocation != null)
            {
                throw new DuplicateLocationNameException(request.Name, request.WarehouseId);
            }

            var location = new Location
            {
                Name = request.Name,
                Description = request.Description,
                WarehouseId = request.WarehouseId,
                OrganizationId = warehouse.OrganizationId
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Locations.Add(location);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Created location {LocationId} with name '{LocationName}' for warehouse {WarehouseId}", 
                    location.Id, location.Name, location.WarehouseId);

                return location;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating location with name '{LocationName}' for warehouse {WarehouseId}", 
                    request.Name, request.WarehouseId);
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
            _logger.LogError(ex, "Database update error while creating location for warehouse {WarehouseId}", request.WarehouseId);
            throw new DatabaseOperationException("Database operation failed while creating location.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is UnauthorizedLocationAccessException || ex is WarehouseNotFoundException || ex is DuplicateLocationNameException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while creating location for warehouse {WarehouseId}", request.WarehouseId);
            throw new DatabaseOperationException("An unexpected error occurred while creating location.", ex);
        }
    }

    public async Task<Location> UpdateLocationAsync(Guid locationId, UpdateLocationRequestDto request, string userId)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Get existing location
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == locationId);

            if (location == null)
            {
                throw new LocationNotFoundException(locationId);
            }

            // Verify user has access to the location's warehouse
            var hasAccess = await UserHasAccessToWarehouseAsync(location.WarehouseId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedLocationAccessException(locationId, userId);
            }

            // Check for duplicate location name within warehouse (excluding current location)
            var existingLocation = await _context.Locations
                .FirstOrDefaultAsync(l => l.WarehouseId == location.WarehouseId && 
                                        l.Name == request.Name && 
                                        l.Id != locationId);

            if (existingLocation != null)
            {
                throw new DuplicateLocationNameException(request.Name, location.WarehouseId);
            }

            location.Name = request.Name;
            location.Description = request.Description;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Locations.Update(location);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Updated location {LocationId} with name '{LocationName}'", 
                    location.Id, location.Name);

                return location;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating location {LocationId}", locationId);
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
            _logger.LogError(ex, "Database update error while updating location {LocationId}", locationId);
            throw new DatabaseOperationException("Database operation failed while updating location.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is LocationNotFoundException || ex is UnauthorizedLocationAccessException || ex is DuplicateLocationNameException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while updating location {LocationId}", locationId);
            throw new DatabaseOperationException("An unexpected error occurred while updating location.", ex);
        }
    }

    public async Task DeleteLocationAsync(Guid locationId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Get existing location
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == locationId);

            if (location == null)
            {
                throw new LocationNotFoundException(locationId);
            }

            // Verify user has access to the location's warehouse
            var hasAccess = await UserHasAccessToWarehouseAsync(location.WarehouseId, userId);
            if (!hasAccess)
            {
                throw new UnauthorizedLocationAccessException(locationId, userId);
            }

            // Check if location has any associated inventory records
            //var hasInventory = await _context.Inventory
            //    .AnyAsync(i => i.LocationId == locationId);

            //if (hasInventory)
            //{
            //    throw new LocationHasInventoryException(locationId);
            //}

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Deleted location {LocationId} with name '{LocationName}'", 
                    location.Id, location.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting location {LocationId}", locationId);
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
            _logger.LogError(ex, "Database update error while deleting location {LocationId}", locationId);
            throw new DatabaseOperationException("Database operation failed while deleting location.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is LocationNotFoundException || ex is UnauthorizedLocationAccessException || ex is LocationHasInventoryException || ex is DatabaseOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting location {LocationId}", locationId);
            throw new DatabaseOperationException("An unexpected error occurred while deleting location.", ex);
        }
    }

    public async Task<bool> UserHasAccessToLocationAsync(Guid locationId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            var hasAccess = await _context.Locations
                .Join(_context.Warehouses,
                    l => l.WarehouseId,
                    w => w.Id,
                    (l, w) => new { Location = l, Warehouse = w })
                .Join(_context.OrganizationMembers,
                    x => x.Warehouse.OrganizationId,
                    om => om.OrganizationId,
                    (x, om) => new { x.Location, x.Warehouse, Member = om })
                .AnyAsync(x => x.Location.Id == locationId && x.Member.UserId == userGuid);

            return hasAccess;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user access to location {LocationId} for user {UserId}", locationId, userId);
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
