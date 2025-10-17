using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using _10xWarehouseNet.Dtos.OrganizationDtos;
using _10xWarehouseNet.Services;
using _10xWarehouseNet.Exceptions;

namespace _10xWarehouseNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IOrganizationService organizationService, IUserService userService, ILogger<UsersController> logger)
        {
            _organizationService = organizationService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                // Get user profile from Supabase (includes display name from metadata)
                var userProfile = await _userService.GetUserProfileAsync(userId);
                
                // Get user memberships from database
                var memberships = await _organizationService.GetUserMembershipsAsync(userId);

                var userDto = new UserDto
                {
                    Id = userId,
                    Email = userProfile.Email,
                    DisplayName = userProfile.DisplayName,
                    Memberships = memberships.ToList()
                };

                return Ok(userDto);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for user data retrieval: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while retrieving user data for user {UserId}", userId);
                return StatusCode(500, "A database error occurred while retrieving user data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving user data for user {UserId}", userId);
                return StatusCode(500, "An unexpected error occurred while retrieving user data.");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required.");
            }

            if (limit <= 0 || limit > 50)
            {
                return BadRequest("Limit must be between 1 and 50.");
            }

            try
            {
                var users = await _userService.SearchUsersAsync(query, limit);
                return Ok(users);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while searching users with query '{Query}'", query);
                return StatusCode(500, "A database error occurred while searching users.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while searching users with query '{Query}'", query);
                return StatusCode(500, "An unexpected error occurred while searching users.");
            }
        }
    }
}
