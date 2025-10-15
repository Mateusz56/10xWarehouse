using _10xWarehouseNet.Dtos;
using _10xWarehouseNet.Dtos.OrganizationDtos;
using _10xWarehouseNet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _10xWarehouseNet.Controllers;

/// <summary>
/// Controller for managing stock movements and inventory operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class StockMovementsController : ControllerBase
{
    private readonly IStockMovementService _stockMovementService;
    private readonly ILogger<StockMovementsController> _logger;

    public StockMovementsController(IStockMovementService stockMovementService, ILogger<StockMovementsController> logger)
    {
        _stockMovementService = stockMovementService;
        _logger = logger;
    }

    /// <summary>
    /// Gets paginated stock movements for an organization with optional filtering
    /// </summary>
    /// <param name="organizationId">The organization ID</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="productTemplateId">Optional filter by product template</param>
    /// <param name="locationId">Optional filter by location (from or to)</param>
    /// <returns>Paginated stock movements</returns>
    [HttpGet]
    public async Task<IActionResult> GetStockMovements(
        [FromQuery] Guid organizationId,
        [FromQuery] PaginationRequestDto pagination,
        [FromQuery] Guid? productTemplateId = null,
        [FromQuery] Guid? locationId = null)
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
            var result = await _stockMovementService.GetStockMovementsAsync(
                organizationId, 
                pagination, 
                productTemplateId, 
                locationId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock movements for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving stock movements.");
        }
    }

    /// <summary>
    /// Creates a new stock movement and updates inventory levels
    /// </summary>
    /// <param name="organizationId">The organization ID</param>
    /// <param name="command">The stock movement command</param>
    /// <returns>The created stock movement</returns>
    [HttpPost]
    [Authorize(Policy = "OwnerOrMember")]
    public async Task<IActionResult> CreateStockMovement(
        [FromQuery] Guid organizationId,
        [FromBody] CreateStockMovementCommand command)
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
            var result = await _stockMovementService.CreateStockMovementAsync(organizationId, userId, command);
            return CreatedAtAction(nameof(GetStockMovements), new { organizationId }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule validation failed for stock movement in organization {OrganizationId}", organizationId);
            
            // Check for specific error types to return appropriate status codes
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            if (ex.Message.Contains("Insufficient inventory"))
            {
                return Conflict(ex.Message);
            }
            
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for stock movement in organization {OrganizationId}", organizationId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stock movement for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while creating the stock movement.");
        }
    }
}
