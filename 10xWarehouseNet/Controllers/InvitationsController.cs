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
    public class InvitationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<InvitationsController> _logger;

        public InvitationsController(IOrganizationService organizationService, ILogger<InvitationsController> logger)
        {
            _organizationService = organizationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserInvitations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var invitations = await _organizationService.GetUserInvitationsAsync(userId);
                var response = new { data = invitations };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get invitations for user {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred while retrieving invitations.");
            }
        }

        [HttpPost("{invitationId}/accept")]
        public async Task<IActionResult> AcceptInvitation(Guid invitationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                await _organizationService.AcceptInvitationAsync(invitationId, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation accepting invitation {InvitationId} for user {UserId}: {Message}", invitationId, userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not authorized to accept invitation {InvitationId}: {Message}", userId, invitationId, ex.Message);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to accept invitation {InvitationId} for user {UserId}", invitationId, userId);
                return StatusCode(500, "An unexpected error occurred while accepting invitation.");
            }
        }

        [HttpPost("{invitationId}/decline")]
        public async Task<IActionResult> DeclineInvitation(Guid invitationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                await _organizationService.DeclineInvitationAsync(invitationId, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation declining invitation {InvitationId} for user {UserId}: {Message}", invitationId, userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not authorized to decline invitation {InvitationId}: {Message}", userId, invitationId, ex.Message);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decline invitation {InvitationId} for user {UserId}", invitationId, userId);
                return StatusCode(500, "An unexpected error occurred while declining invitation.");
            }
        }
    }
}
