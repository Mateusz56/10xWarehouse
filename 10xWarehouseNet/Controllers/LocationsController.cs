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
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationsController> _logger;

        public LocationsController(ILocationService locationService, ILogger<LocationsController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        /// <summary>
        /// Gets locations belonging to a specific warehouse with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLocations([FromQuery] Guid warehouseId, [FromQuery] PaginationRequestDto pagination)
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
                var (locations, totalCount) = await _locationService.GetWarehouseLocationsAsync(
                    warehouseId, userId, pagination.Page, pagination.PageSize);

                var locationDtos = locations.Select(l => new LocationDto(l.Id, l.Name, l.Description, l.WarehouseId));
                var paginationDto = new PaginationDto(pagination.Page, pagination.PageSize, totalCount);
                var response = new PaginatedResponseDto<LocationDto>(locationDtos, paginationDto);

                return Ok(response);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for location retrieval: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (InvalidPaginationException ex)
            {
                _logger.LogWarning(ex, "Invalid pagination parameters for user {UserId}: page={Page}, pageSize={PageSize}", userId, pagination.Page, pagination.PageSize);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedLocationAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to warehouse {WarehouseId}", userId, warehouseId);
                return Forbid("You don't have access to this warehouse.");
            }
            catch (WarehouseNotFoundException ex)
            {
                _logger.LogWarning(ex, "Warehouse {WarehouseId} not found for user {UserId}", warehouseId, userId);
                return NotFound(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while retrieving locations for warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, "A database error occurred while retrieving locations.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving locations for warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, "An unexpected error occurred while retrieving locations.");
            }
        }

        /// <summary>
        /// Gets a single location by ID
        /// </summary>
        [HttpGet("{locationId}")]
        public async Task<IActionResult> GetLocation(Guid locationId)
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
                var location = await _locationService.GetLocationByIdAsync(locationId, userId);

                if (location == null)
                {
                    return NotFound($"Location with ID {locationId} was not found.");
                }

                var locationDto = new LocationDto(location.Id, location.Name, location.Description, location.WarehouseId);

                return Ok(locationDto);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for location retrieval: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (UnauthorizedLocationAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to location {LocationId}", userId, locationId);
                return Forbid("You don't have access to this location.");
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while retrieving location {LocationId}", locationId);
                return StatusCode(500, "A database error occurred while retrieving the location.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving location {LocationId}", locationId);
                return StatusCode(500, "An unexpected error occurred while retrieving the location.");
            }
        }

        /// <summary>
        /// Creates a new location within a warehouse
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLocation([FromBody] CreateLocationRequestDto request)
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
                var location = await _locationService.CreateLocationAsync(request, userId);

                var locationDto = new LocationDto(location.Id, location.Name, location.Description, location.WarehouseId);

                return CreatedAtAction(nameof(GetLocation), new { locationId = location.Id }, locationDto);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for location creation: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (UnauthorizedLocationAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to warehouse {WarehouseId}", userId, request.WarehouseId);
                return Forbid("You don't have access to this warehouse.");
            }
            catch (WarehouseNotFoundException ex)
            {
                _logger.LogWarning(ex, "Warehouse {WarehouseId} not found for location creation", request.WarehouseId);
                return NotFound(ex.Message);
            }
            catch (DuplicateLocationNameException ex)
            {
                _logger.LogWarning(ex, "Duplicate location name '{LocationName}' in warehouse {WarehouseId}", request.Name, request.WarehouseId);
                return Conflict(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while creating location for warehouse {WarehouseId}", request.WarehouseId);
                return StatusCode(500, "A database error occurred while creating the location.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating location for warehouse {WarehouseId}", request.WarehouseId);
                return StatusCode(500, "An unexpected error occurred while creating the location.");
            }
        }

        /// <summary>
        /// Updates an existing location
        /// </summary>
        [HttpPut("{locationId}")]
        public async Task<IActionResult> UpdateLocation(Guid locationId, [FromBody] UpdateLocationRequestDto request)
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
                var location = await _locationService.UpdateLocationAsync(locationId, request, userId);

                var locationDto = new LocationDto(location.Id, location.Name, location.Description, location.WarehouseId);

                return Ok(locationDto);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for location update: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (LocationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Location {LocationId} not found for update", locationId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedLocationAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to location {LocationId}", userId, locationId);
                return Forbid("You don't have access to this location.");
            }
            catch (DuplicateLocationNameException ex)
            {
                _logger.LogWarning(ex, "Duplicate location name '{LocationName}' during update", request.Name);
                return Conflict(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while updating location {LocationId}", locationId);
                return StatusCode(500, "A database error occurred while updating the location.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating location {LocationId}", locationId);
                return StatusCode(500, "An unexpected error occurred while updating the location.");
            }
        }

        /// <summary>
        /// Deletes a location (only if it has no inventory)
        /// </summary>
        [HttpDelete("{locationId}")]
        public async Task<IActionResult> DeleteLocation(Guid locationId)
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
                await _locationService.DeleteLocationAsync(locationId, userId);

                return Ok();
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID for location deletion: {UserId}", userId);
                return Unauthorized("Invalid user authentication.");
            }
            catch (LocationNotFoundException ex)
            {
                _logger.LogWarning(ex, "Location {LocationId} not found for deletion", locationId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedLocationAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to location {LocationId}", userId, locationId);
                return Forbid("You don't have access to this location.");
            }
            catch (LocationHasInventoryException ex)
            {
                _logger.LogWarning(ex, "Cannot delete location {LocationId} because it has inventory", locationId);
                return Conflict(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while deleting location {LocationId}", locationId);
                return StatusCode(500, "A database error occurred while deleting the location.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting location {LocationId}", locationId);
                return StatusCode(500, "An unexpected error occurred while deleting the location.");
            }
        }
    }
}
