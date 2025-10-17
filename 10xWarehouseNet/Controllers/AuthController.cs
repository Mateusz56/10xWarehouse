using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Exceptions;
using _10xWarehouseNet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supabase.Gotrue.Exceptions;
using System.Security.Claims;

namespace _10xWarehouseNet.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                // Register user with Supabase and create organization if requested
                var result = await _userService.RegisterUserAsync(request);

                return CreatedAtAction("Register", new { }, result);
            }
            catch (GotrueException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in registration request");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in registration request");
                return Conflict(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error during user registration");
                return StatusCode(500, "A database error occurred during registration.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user registration");
                return StatusCode(500, "An unexpected error occurred during registration.");
            }
        }


        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserProfileRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = User.FindFirstValue(ClaimTypes.Email);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }

                var updatedProfile = await _userService.UpdateUserProfileAsync(userId, request);
                
                // Set email from JWT token
                updatedProfile.Email = email ?? "";

                return Ok(updatedProfile);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in profile update request");
                return BadRequest(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error during profile update");
                return StatusCode(500, "A database error occurred during profile update.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during profile update");
                return StatusCode(500, "An unexpected error occurred during profile update.");
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }

                await _userService.ChangeUserPasswordAsync(userId, request);
                
                return Ok(new { message = "Password changed successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in password change request");
                return BadRequest(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error during password change");
                return StatusCode(500, "A database error occurred during password change.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password change");
                return StatusCode(500, "An unexpected error occurred during password change.");
            }
        }
    }
}
