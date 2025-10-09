using _10xWarehouseNet.Dtos.OrganizationDtos;
using _10xWarehouseNet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _10xWarehouseNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<OrganizationsController> _logger;

        public OrganizationsController(IOrganizationService organizationService, ILogger<OrganizationsController> logger)
        {
            _organizationService = organizationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = "012f67d6-a528-4d0e-afa6-29d957e31f25";// User.FindFirstValue(ClaimTypes.NameIdentifier);
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
    }
}
