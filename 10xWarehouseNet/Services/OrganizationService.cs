using _10xWarehouseNet.Db;
using _10xWarehouseNet.Db.Enums;
using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos.OrganizationDtos;
using _10xWarehouseNet.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace _10xWarehouseNet.Services;

public class OrganizationService : IOrganizationService
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(WarehouseDbContext context, ILogger<OrganizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Organization> CreateOrganizationAsync(CreateOrganizationRequestDto request, string userId)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }

        var organization = new Organization
        {
            Name = request.Name
        };

        var member = new OrganizationMember
        {
            Organization = organization,
            UserId = Guid.Parse(userId),
            Role = UserRole.Owner
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Organizations.Add(organization);
            _context.OrganizationMembers.Add(member);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return organization;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating organization with name {OrganizationName}", request.Name);
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(IEnumerable<Organization> organizations, int totalCount)> GetUserOrganizationsAsync(string userId, int page, int pageSize)
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

            // Get total count for pagination
            var totalCount = await _context.OrganizationMembers
                .Where(om => om.UserId == userGuid)
                .CountAsync();

            // Get paginated organizations
            var organizations = await _context.OrganizationMembers
                .Where(om => om.UserId == userGuid)
                .Include(om => om.Organization)
                .OrderBy(om => om.Organization.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(om => om.Organization)
                .ToListAsync();

            return (organizations, totalCount);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while retrieving organizations for user {UserId}", userId);
            throw new DatabaseOperationException("Database operation failed while retrieving organizations.", ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving organizations for user {UserId}", userId);
            throw new DatabaseOperationException("Invalid database operation while retrieving organizations.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving organizations for user {UserId}", userId);
            throw new DatabaseOperationException("An unexpected error occurred while retrieving organizations.", ex);
        }
    }

    public async Task<IEnumerable<MembershipDto>> GetUserMembershipsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            var memberships = await _context.OrganizationMembers
                .Where(om => om.UserId == userGuid)
                .Include(om => om.Organization)
                .Select(om => new MembershipDto
                {
                    OrganizationId = om.OrganizationId.ToString(),
                    OrganizationName = om.Organization.Name,
                    Role = om.Role.ToString()
                })
                .ToListAsync();

            return memberships;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while retrieving memberships for user {UserId}", userId);
            throw new DatabaseOperationException("Database operation failed while retrieving memberships.", ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving memberships for user {UserId}", userId);
            throw new DatabaseOperationException("Invalid database operation while retrieving memberships.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving memberships for user {UserId}", userId);
            throw new DatabaseOperationException("An unexpected error occurred while retrieving memberships.", ex);
        }
    }
}
