using _10xWarehouseNet.Db;
using _10xWarehouseNet.Db.Enums;
using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos.OrganizationDtos;

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
}
