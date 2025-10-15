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
    [Authorize(Policy = "OrganizationMember")] // Require organization membership for all endpoints
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a paginated summary of inventory levels across all warehouses and locations within the specified organization
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInventory(
            [FromQuery] Guid organizationId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] Guid? locationId = null,
            [FromQuery] Guid? productTemplateId = null,
            [FromQuery] bool? lowStock = null)
        {
            // Validate pagination parameters
            if (page < 1)
            {
                return BadRequest("Page must be greater than 0.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100.");
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
                var (inventoryItems, totalCount) = await _inventoryService.GetInventorySummaryAsync(
                    organizationId, userId, page, pageSize, locationId, productTemplateId, lowStock);

                var paginationDto = new PaginationDto(page, pageSize, totalCount);
                var response = new PaginatedResponseDto<InventorySummaryDto>(inventoryItems, paginationDto);

                return Ok(response);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for inventory retrieval: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (InvalidPaginationException ex)
            {
                _logger.LogWarning(ex, "Invalid pagination parameters for user {UserId}: page={Page}, pageSize={PageSize}", userId, page, pageSize);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedInventoryAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to organization {OrganizationId}", userId, organizationId);
                return Forbid("You don't have access to this organization.");
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while retrieving inventory for organization {OrganizationId}", organizationId);
                return StatusCode(500, "A database error occurred while retrieving inventory.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving inventory for organization {OrganizationId}", organizationId);
                return StatusCode(500, "An unexpected error occurred while retrieving inventory.");
            }
        }
    }
}
