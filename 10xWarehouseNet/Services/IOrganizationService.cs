using _10xWarehouseNet.Db.Models;
using _10xWarehouseNet.Dtos.OrganizationDtos;

namespace _10xWarehouseNet.Services;

public interface IOrganizationService
{
    Task<Organization> CreateOrganizationAsync(CreateOrganizationRequestDto request, string userId);
    Task<(IEnumerable<Organization> organizations, int totalCount)> GetUserOrganizationsAsync(string userId, int page, int pageSize);
    Task<IEnumerable<MembershipDto>> GetUserMembershipsAsync(string userId);
    Task<Organization> UpdateOrganizationAsync(Guid organizationId, UpdateOrganizationRequestDto request, string userId);
    Task<(IEnumerable<OrganizationMemberDto> members, int totalCount)> GetOrganizationMembersAsync(Guid organizationId, string userId, int page, int pageSize);
    Task<Invitation> CreateInvitationAsync(Guid organizationId, CreateInvitationRequestDto request, string userId);
    Task RemoveOrganizationMemberAsync(Guid organizationId, Guid targetUserId, string userId);
    Task CancelInvitationAsync(Guid organizationId, Guid invitationId, string userId);
    Task<IEnumerable<UserInvitationDto>> GetUserInvitationsAsync(string userId);
    Task<(IEnumerable<InvitationDto> invitations, int totalCount)> GetOrganizationInvitationsAsync(Guid organizationId, string userId, int page, int pageSize);
    Task AcceptInvitationAsync(Guid invitationId, string userId);
    Task DeclineInvitationAsync(Guid invitationId, string userId);
}
