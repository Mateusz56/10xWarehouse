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
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehousesController> _logger;

        public WarehousesController(IWarehouseService warehouseService, ILogger<WarehousesController> logger)
        {
            _warehouseService = warehouseService;
            _logger = logger;
        }

        /// <summary>
        /// Gets warehouses belonging to a specific organization with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWarehouses([FromQuery] Guid organizationId, [FromQuery] PaginationRequestDto pagination)
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
                var (warehouses, totalCount) = await _warehouseService.GetOrganizationWarehousesAsync(
                    organizationId, userId, pagination.Page, pagination.PageSize);

                var warehouseDtos = warehouses.Select(w => new WarehouseDto(w.Id, w.Name, w.OrganizationId));
                var paginationDto = new PaginationDto(pagination.Page, pagination.PageSize, totalCount);
                var response = new PaginatedResponseDto<WarehouseDto>(warehouseDtos, paginationDto);

                return Ok(response);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for warehouse retrieval: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (InvalidPaginationException ex)
            {
                _logger.LogWarning(ex, "Invalid pagination parameters for user {UserId}: page={Page}, pageSize={PageSize}", userId, pagination.Page, pagination.PageSize);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedWarehouseAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to organization {OrganizationId}", userId, organizationId);
                return Forbid("You don't have access to this organization.");
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while retrieving warehouses for organization {OrganizationId}", organizationId);
                return StatusCode(500, "A database error occurred while retrieving warehouses.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving warehouses for organization {OrganizationId}", organizationId);
                return StatusCode(500, "An unexpected error occurred while retrieving warehouses.");
            }
        }

        /// <summary>
        /// Gets a single warehouse by ID with its locations
        /// </summary>
        [HttpGet("{warehouseId}")]
        public async Task<IActionResult> GetWarehouse(Guid warehouseId)
        {
            // Get user ID from authenticated claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims");
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var warehouse = await _warehouseService.GetWarehouseByIdAsync(warehouseId, userId);

                if (warehouse == null)
                {
                    return NotFound($"Warehouse with ID {warehouseId} was not found.");
                }

                var locationDtos = warehouse.Locations.Select(l => new LocationDto(l.Id, l.Name, l.Description, l.OrganizationId)).ToList();
                var warehouseDto = new WarehouseWithLocationsDto(warehouse.Id, warehouse.Name, warehouse.OrganizationId, locationDtos);

                return Ok(warehouseDto);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for warehouse retrieval: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (UnauthorizedWarehouseAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to warehouse {WarehouseId}", userId, warehouseId);
                return Forbid("You don't have access to this warehouse.");
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while retrieving warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, "A database error occurred while retrieving the warehouse.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, "An unexpected error occurred while retrieving the warehouse.");
            }
        }

        /// <summary>
        /// Creates a new warehouse
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequestDto request)
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
                var warehouse = await _warehouseService.CreateWarehouseAsync(request, userId);

                var warehouseDto = new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.OrganizationId);

                return CreatedAtAction(nameof(GetWarehouse), new { warehouseId = warehouse.Id }, warehouseDto);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for warehouse creation: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (UnauthorizedWarehouseAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to organization {OrganizationId}", userId, request.OrganizationId);
                return Forbid("You don't have access to this organization.");
            }
            catch (DuplicateWarehouseNameException ex)
            {
                _logger.LogWarning(ex, "Duplicate warehouse name '{WarehouseName}' in organization {OrganizationId}", request.Name, request.OrganizationId);
                return BadRequest(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while creating warehouse for organization {OrganizationId}", request.OrganizationId);
                return StatusCode(500, "A database error occurred while creating the warehouse.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating warehouse for organization {OrganizationId}", request.OrganizationId);
                return StatusCode(500, "An unexpected error occurred while creating the warehouse.");
            }
        }

        /// <summary>
        /// Updates an existing warehouse
        /// </summary>
        [HttpPut("{warehouseId}")]
        public async Task<IActionResult> UpdateWarehouse(Guid warehouseId, [FromBody] UpdateWarehouseRequestDto request)
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
                var warehouse = await _warehouseService.UpdateWarehouseAsync(warehouseId, request, userId);

                var warehouseDto = new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.OrganizationId);

                return Ok(warehouseDto);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for warehouse update: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (WarehouseNotFoundException ex)
            {
                _logger.LogWarning(ex, "Warehouse {WarehouseId} not found for update", warehouseId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedWarehouseAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to warehouse {WarehouseId}", userId, warehouseId);
                return Forbid("You don't have access to this warehouse.");
            }
            catch (DuplicateWarehouseNameException ex)
            {
                _logger.LogWarning(ex, "Duplicate warehouse name '{WarehouseName}' during update", request.Name);
                return BadRequest(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while updating warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, "A database error occurred while updating the warehouse.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, "An unexpected error occurred while updating the warehouse.");
            }
        }

        /// <summary>
        /// Deletes a warehouse (only if it has no locations)
        /// </summary>
        [HttpDelete("{warehouseId}")]
        public async Task<IActionResult> DeleteWarehouse(Guid warehouseId)
        {
            // Get user ID from authenticated claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims");
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                await _warehouseService.DeleteWarehouseAsync(warehouseId, userId);

                return Ok();
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for warehouse deletion: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (WarehouseNotFoundException ex)
            {
                _logger.LogWarning(ex, "Warehouse {WarehouseId} not found for deletion", warehouseId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedWarehouseAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to warehouse {WarehouseId}", userId, warehouseId);
                return Forbid("You don't have access to this warehouse.");
            }
            catch (WarehouseHasLocationsException ex)
            {
                _logger.LogWarning(ex, "Cannot delete warehouse {WarehouseId} because it has locations", warehouseId);
                return Conflict(ex.Message);
            }
            catch (WarehouseHasLocationsWithInventoryException ex)
            {
                _logger.LogWarning(ex, "Cannot delete warehouse {WarehouseId} because it has locations with inventory", warehouseId);
                return Conflict(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while deleting warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, "A database error occurred while deleting the warehouse.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, "An unexpected error occurred while deleting the warehouse.");
            }
        }
    }
}
