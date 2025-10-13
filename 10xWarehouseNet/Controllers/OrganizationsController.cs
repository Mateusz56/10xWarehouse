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
    }
}
