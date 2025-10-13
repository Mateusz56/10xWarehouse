using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using _10xWarehouseNet.Dtos.OrganizationDtos;

namespace _10xWarehouseNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var displayName = User.FindFirstValue("metadata.display_name") ?? email;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            // This is a simplified response. In a real app, you'd fetch from your database
            var userDto = new UserDto
            {
                Id = userId,
                Email = email ?? "",
                DisplayName = displayName,
                Memberships = new List<MembershipDto>() // You'd populate this from your database
            };

            return Ok(userDto);
        }
    }
}
