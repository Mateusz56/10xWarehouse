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

    public async Task<Organization> UpdateOrganizationAsync(Guid organizationId, UpdateOrganizationRequestDto request, string userId)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Check if organization exists and user is owner
            var organization = await _context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new OrganizationNotFoundException($"Organization with ID {organizationId} not found.");
            }

            var userMembership = organization.Members.FirstOrDefault(m => m.UserId == userGuid && m.Role == UserRole.Owner);
            if (userMembership == null)
            {
                throw new UnauthorizedAccessException("User is not authorized to update this organization.");
            }

            organization.Name = request.Name;
            await _context.SaveChangesAsync();

            return organization;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while updating organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("Database operation failed while updating organization.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is OrganizationNotFoundException || ex is UnauthorizedAccessException))
        {
            _logger.LogError(ex, "An unexpected error occurred while updating organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("An unexpected error occurred while updating organization.", ex);
        }
    }

    public async Task<(IEnumerable<OrganizationMemberDto> members, int totalCount)> GetOrganizationMembersAsync(Guid organizationId, string userId, int page, int pageSize)
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

            // Check if organization exists and user is a member
            var organization = await _context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new OrganizationNotFoundException($"Organization with ID {organizationId} not found.");
            }

            var userMembership = organization.Members.FirstOrDefault(m => m.UserId == userGuid);
            if (userMembership == null)
            {
                throw new UnauthorizedAccessException("User is not authorized to view this organization's members.");
            }

            // Get members and invitations
            var members = await _context.OrganizationMembers
                .Where(om => om.OrganizationId == organizationId)
                .Select(om => new OrganizationMemberDto(om.UserId, "", om.Role, InvitationStatus.Accepted))
                .ToListAsync();

            var invitations = await _context.Invitations
                .Where(i => i.OrganizationId == organizationId && i.Status == InvitationStatus.Pending)
                .Select(i => new OrganizationMemberDto(i.InvitedUserId, "", i.Role, InvitationStatus.Pending))
                .ToListAsync();

            var allMembers = members.Concat(invitations).ToList();
            var totalCount = allMembers.Count;

            var paginatedMembers = allMembers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (paginatedMembers, totalCount);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while retrieving organization members for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("Database operation failed while retrieving organization members.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is OrganizationNotFoundException || ex is UnauthorizedAccessException || ex is InvalidPaginationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving organization members for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("An unexpected error occurred while retrieving organization members.", ex);
        }
    }

    public async Task<Invitation> CreateInvitationAsync(Guid organizationId, CreateInvitationRequestDto request, string userId)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Check if organization exists and user is owner
            var organization = await _context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new OrganizationNotFoundException($"Organization with ID {organizationId} not found.");
            }

            var userMembership = organization.Members.FirstOrDefault(m => m.UserId == userGuid && m.Role == UserRole.Owner);
            if (userMembership == null)
            {
                throw new UnauthorizedAccessException("User is not authorized to create invitations for this organization.");
            }

            // Check if invitation already exists
            var existingInvitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.OrganizationId == organizationId && i.InvitedUserId == request.InvitedUserId);

            if (existingInvitation != null)
            {
                throw new InvalidOperationException("Invitation already exists for this user.");
            }

            // Check if user is already a member
            var existingMember = organization.Members.FirstOrDefault(m => m.UserId == request.InvitedUserId);
            if (existingMember != null)
            {
                throw new InvalidOperationException("User is already a member of this organization.");
            }

            var invitation = new Invitation
            {
                OrganizationId = organizationId,
                InvitedUserId = request.InvitedUserId,
                Role = request.Role,
                Status = InvitationStatus.Pending
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            return invitation;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while creating invitation for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("Database operation failed while creating invitation.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is OrganizationNotFoundException || ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while creating invitation for organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("An unexpected error occurred while creating invitation.", ex);
        }
    }

    public async Task RemoveOrganizationMemberAsync(Guid organizationId, Guid targetUserId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Check if organization exists and user is owner
            var organization = await _context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new OrganizationNotFoundException($"Organization with ID {organizationId} not found.");
            }

            var userMembership = organization.Members.FirstOrDefault(m => m.UserId == userGuid && m.Role == UserRole.Owner);
            if (userMembership == null)
            {
                throw new UnauthorizedAccessException("User is not authorized to remove members from this organization.");
            }

            // Check if target user is a member
            var targetMembership = organization.Members.FirstOrDefault(m => m.UserId == targetUserId);
            if (targetMembership == null)
            {
                throw new InvalidOperationException("User is not a member of this organization.");
            }

            // Check if removing would leave organization without owners
            var ownerCount = organization.Members.Count(m => m.Role == UserRole.Owner);
            if (targetMembership.Role == UserRole.Owner && ownerCount <= 1)
            {
                throw new InvalidOperationException("Cannot remove the last owner of the organization.");
            }

            _context.OrganizationMembers.Remove(targetMembership);
            await _context.SaveChangesAsync();
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while removing member from organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("Database operation failed while removing member.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is OrganizationNotFoundException || ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while removing member from organization {OrganizationId}", organizationId);
            throw new DatabaseOperationException("An unexpected error occurred while removing member.", ex);
        }
    }

    public async Task CancelInvitationAsync(Guid organizationId, Guid invitationId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Check if organization exists and user is owner
            var organization = await _context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization == null)
            {
                throw new OrganizationNotFoundException($"Organization with ID {organizationId} not found.");
            }

            var userMembership = organization.Members.FirstOrDefault(m => m.UserId == userGuid && m.Role == UserRole.Owner);
            if (userMembership == null)
            {
                throw new UnauthorizedAccessException("User is not authorized to cancel invitations for this organization.");
            }

            // Check if invitation exists and belongs to organization
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Id == invitationId && i.OrganizationId == organizationId);

            if (invitation == null)
            {
                throw new InvalidOperationException("Invitation not found.");
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                throw new InvalidOperationException("Cannot cancel an invitation that has already been processed.");
            }

            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync();
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while canceling invitation {InvitationId}", invitationId);
            throw new DatabaseOperationException("Database operation failed while canceling invitation.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is OrganizationNotFoundException || ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while canceling invitation {InvitationId}", invitationId);
            throw new DatabaseOperationException("An unexpected error occurred while canceling invitation.", ex);
        }
    }

    public async Task<IEnumerable<UserInvitationDto>> GetUserInvitationsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            var invitations = await _context.Invitations
                .Where(i => i.InvitedUserId == userGuid && i.Status == InvitationStatus.Pending)
                .Include(i => i.Organization)
                .Select(i => new UserInvitationDto
                {
                    Id = i.Id,
                    OrganizationName = i.Organization.Name,
                    Role = i.Role,
                    InvitedAt = DateTime.UtcNow // Note: You may want to add CreatedAt field to Invitation model
                })
                .ToListAsync();

            return invitations;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while retrieving invitations for user {UserId}", userId);
            throw new DatabaseOperationException("Database operation failed while retrieving invitations.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException))
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving invitations for user {UserId}", userId);
            throw new DatabaseOperationException("An unexpected error occurred while retrieving invitations.", ex);
        }
    }

    public async Task AcceptInvitationAsync(Guid invitationId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Check if invitation exists and is pending
            var invitation = await _context.Invitations
                .Include(i => i.Organization)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null)
            {
                throw new InvalidOperationException("Invitation not found.");
            }

            if (invitation.InvitedUserId != userGuid)
            {
                throw new UnauthorizedAccessException("User is not authorized to accept this invitation.");
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                throw new InvalidOperationException("Invitation has already been processed.");
            }

            // Check if user is already a member
            var existingMember = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == invitation.OrganizationId && om.UserId == userGuid);

            if (existingMember != null)
            {
                throw new InvalidOperationException("User is already a member of this organization.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create organization member
                var member = new OrganizationMember
                {
                    OrganizationId = invitation.OrganizationId,
                    UserId = userGuid,
                    Role = invitation.Role
                };

                _context.OrganizationMembers.Add(member);

                // Update invitation status
                invitation.Status = InvitationStatus.Accepted;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
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
            _logger.LogError(ex, "Database update error while accepting invitation {InvitationId}", invitationId);
            throw new DatabaseOperationException("Database operation failed while accepting invitation.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while accepting invitation {InvitationId}", invitationId);
            throw new DatabaseOperationException("An unexpected error occurred while accepting invitation.", ex);
        }
    }

    public async Task DeclineInvitationAsync(Guid invitationId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidUserIdException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Check if invitation exists and is pending
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null)
            {
                throw new InvalidOperationException("Invitation not found.");
            }

            if (invitation.InvitedUserId != userGuid)
            {
                throw new UnauthorizedAccessException("User is not authorized to decline this invitation.");
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                throw new InvalidOperationException("Invitation has already been processed.");
            }

            // Update invitation status to declined
            invitation.Status = InvitationStatus.Declined;
            await _context.SaveChangesAsync();
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid user ID format: {UserId}", userId);
            throw new InvalidUserIdException("Invalid user ID format.", nameof(userId), ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while declining invitation {InvitationId}", invitationId);
            throw new DatabaseOperationException("Database operation failed while declining invitation.", ex);
        }
        catch (Exception ex) when (!(ex is InvalidUserIdException || ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred while declining invitation {InvitationId}", invitationId);
            throw new DatabaseOperationException("An unexpected error occurred while declining invitation.", ex);
        }
    }
}
