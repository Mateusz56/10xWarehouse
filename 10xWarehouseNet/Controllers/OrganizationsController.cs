using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Dtos.OrganizationDtos;
using _10xWarehouseNet.Exceptions;
using _10xWarehouseNet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _10xWarehouseNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<OrganizationsController> _logger;

        public OrganizationsController(IOrganizationService organizationService, ILogger<OrganizationsController> logger)
        {
            _organizationService = organizationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganizations([FromQuery] PaginationRequestDto pagination)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get user ID from authenticated claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims");
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var (organizations, totalCount) = await _organizationService.GetUserOrganizationsAsync(userId, pagination.Page, pagination.PageSize);

                var organizationDtos = organizations.Select(org => new OrganizationDto(org.Id, org.Name));
                var paginationDto = new PaginationDto(pagination.Page, pagination.PageSize, totalCount);
                var response = new PaginatedResponseDto<OrganizationDto>(organizationDtos, paginationDto);

                return Ok(response);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for organization retrieval: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (InvalidPaginationException ex)
            {
                _logger.LogWarning(ex, "Invalid pagination parameters for user {UserId}: page={Page}, pageSize={PageSize}", userId, pagination.Page, pagination.PageSize);
                return BadRequest(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while retrieving organizations for user {UserId}", userId);
                return StatusCode(500, "A database error occurred while retrieving organizations.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving organizations for user {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred while retrieving organizations.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get user ID from authenticated claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var organization = await _organizationService.CreateOrganizationAsync(request, userId);

                var organizationDto = new OrganizationDto(organization.Id, organization.Name);

                return Ok(organizationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create organization for user {UserId}", userId);
                return BadRequest("An unexpected error occurred.");
            }
        }

        [HttpPut("{orgId}")]
        public async Task<IActionResult> UpdateOrganization(Guid orgId, [FromBody] UpdateOrganizationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var organization = await _organizationService.UpdateOrganizationAsync(orgId, request, userId);
                var organizationDto = new OrganizationDto(organization.Id, organization.Name);
                return Ok(organizationDto);
            }
            catch (OrganizationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization {OrganizationId} not found for user {UserId}", orgId, userId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not authorized to update organization {OrganizationId}", userId, orgId);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update organization {OrganizationId} for user {UserId}", orgId, userId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("{orgId}/members")]
        public async Task<IActionResult> GetOrganizationMembers(Guid orgId, [FromQuery] PaginationRequestDto pagination)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var (members, totalCount) = await _organizationService.GetOrganizationMembersAsync(orgId, userId, pagination.Page, pagination.PageSize);
                var paginationDto = new PaginationDto(pagination.Page, pagination.PageSize, totalCount);
                var response = new PaginatedResponseDto<OrganizationMemberDto>(members, paginationDto);
                return Ok(response);
            }
            catch (OrganizationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization {OrganizationId} not found for user {UserId}", orgId, userId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not authorized to view members of organization {OrganizationId}", userId, orgId);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get members for organization {OrganizationId} for user {UserId}", orgId, userId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost("{orgId}/invitations")]
        public async Task<IActionResult> CreateInvitation(Guid orgId, [FromBody] CreateInvitationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var invitation = await _organizationService.CreateInvitationAsync(orgId, request, userId);
                var invitationDto = new InvitationDto(invitation.Id, invitation.InvitedUserId, invitation.Role, invitation.Status);
                return StatusCode(201, invitationDto);
            }
            catch (OrganizationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization {OrganizationId} not found for user {UserId}", orgId, userId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not authorized to create invitations for organization {OrganizationId}", userId, orgId);
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation creating invitation for organization {OrganizationId}: {Message}", orgId, ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create invitation for organization {OrganizationId} for user {UserId}", orgId, userId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{orgId}/members/{userId}")]
        public async Task<IActionResult> RemoveOrganizationMember(Guid orgId, Guid userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                await _organizationService.RemoveOrganizationMemberAsync(orgId, userId, currentUserId);
                return Ok(); // Following backend rule to return 200 for delete operations
            }
            catch (OrganizationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization {OrganizationId} not found for user {UserId}", orgId, currentUserId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not authorized to remove members from organization {OrganizationId}", currentUserId, orgId);
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation removing member from organization {OrganizationId}: {Message}", orgId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove member {TargetUserId} from organization {OrganizationId} for user {UserId}", userId, orgId, currentUserId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("{orgId}/invitations/{invitationId}")]
        public async Task<IActionResult> CancelInvitation(Guid orgId, Guid invitationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                await _organizationService.CancelInvitationAsync(orgId, invitationId, userId);
                return Ok(); // Following backend rule to return 200 for delete operations
            }
            catch (OrganizationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization {OrganizationId} not found for user {UserId}", orgId, userId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not authorized to cancel invitations for organization {OrganizationId}", userId, orgId);
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation canceling invitation {InvitationId} for organization {OrganizationId}: {Message}", invitationId, orgId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel invitation {InvitationId} for organization {OrganizationId} for user {UserId}", invitationId, orgId, userId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
