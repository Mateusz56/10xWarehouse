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
    public class ProductTemplatesController : ControllerBase
    {
        private readonly IProductTemplateService _productTemplateService;
        private readonly ILogger<ProductTemplatesController> _logger;

        public ProductTemplatesController(IProductTemplateService productTemplateService, ILogger<ProductTemplatesController> logger)
        {
            _productTemplateService = productTemplateService;
            _logger = logger;
        }

        /// <summary>
        /// Gets product templates belonging to a specific organization with pagination and search
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductTemplates([FromQuery] Guid organizationId, [FromQuery] PaginationRequestDto pagination, [FromQuery] string? search = null)
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
                var (productTemplates, totalCount) = await _productTemplateService.GetOrganizationProductTemplatesAsync(
                    organizationId, userId, pagination.Page, pagination.PageSize, search);

                var productTemplateDtos = productTemplates.Select(pt => new ProductTemplateDto(
                    pt.Id, 
                    pt.Name, 
                    pt.Barcode, 
                    pt.Description, 
                    pt.LowStockThreshold));

                var paginationDto = new PaginationDto(pagination.Page, pagination.PageSize, totalCount);
                var response = new PaginatedResponseDto<ProductTemplateDto>(productTemplateDtos, paginationDto);

                return Ok(response);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID: {UserId}", userId);
                return Unauthorized(ex.Message);
            }
            catch (InvalidPaginationException ex)
            {
                _logger.LogWarning(ex, "Invalid pagination parameters: Page={Page}, PageSize={PageSize}", pagination.Page, pagination.PageSize);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedProductTemplateAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to organization {OrganizationId}", userId, organizationId);
                return Forbid(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while getting product templates for organization {OrganizationId}", organizationId);
                return StatusCode(500, "An error occurred while retrieving product templates.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting product templates for organization {OrganizationId}", organizationId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Gets a single product template by ID
        /// </summary>
        [HttpGet("{productTemplateId}")]
        public async Task<IActionResult> GetProductTemplate(Guid productTemplateId)
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
                var productTemplate = await _productTemplateService.GetProductTemplateByIdAsync(productTemplateId, userId);

                if (productTemplate == null)
                {
                    return NotFound($"Product template with ID {productTemplateId} was not found.");
                }

                var productTemplateDto = new ProductTemplateDto(
                    productTemplate.Id,
                    productTemplate.Name,
                    productTemplate.Barcode,
                    productTemplate.Description,
                    productTemplate.LowStockThreshold);

                return Ok(productTemplateDto);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID: {UserId}", userId);
                return Unauthorized(ex.Message);
            }
            catch (UnauthorizedProductTemplateAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to product template {ProductTemplateId}", userId, productTemplateId);
                return Forbid(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while getting product template {ProductTemplateId}", productTemplateId);
                return StatusCode(500, "An error occurred while retrieving the product template.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting product template {ProductTemplateId}", productTemplateId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Creates a new product template (requires Owner role)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProductTemplate([FromBody] CreateProductTemplateRequestDto request)
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
                var productTemplate = await _productTemplateService.CreateProductTemplateAsync(request, request.OrganizationId, userId);

                var productTemplateDto = new ProductTemplateDto(
                    productTemplate.Id,
                    productTemplate.Name,
                    productTemplate.Barcode,
                    productTemplate.Description,
                    productTemplate.LowStockThreshold);

                return CreatedAtAction(nameof(GetProductTemplate), new { productTemplateId = productTemplate.Id }, productTemplateDto);
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID: {UserId}", userId);
                return Unauthorized(ex.Message);
            }
            catch (UnauthorizedProductTemplateAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to create product template in organization {OrganizationId}", userId, request.OrganizationId);
                return Forbid(ex.Message);
            }
            catch (DuplicateProductTemplateBarcodeException ex)
            {
                _logger.LogWarning(ex, "Duplicate barcode attempt: {Barcode} in organization {OrganizationId}", request.Barcode, request.OrganizationId);
                return Conflict(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while creating product template for organization {OrganizationId}", request.OrganizationId);
                return StatusCode(500, "An error occurred while creating the product template.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating product template for organization {OrganizationId}", request.OrganizationId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Deletes a product template (requires Owner role)
        /// </summary>
        [HttpDelete("{productTemplateId}")]
        public async Task<IActionResult> DeleteProductTemplate(Guid productTemplateId)
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
                await _productTemplateService.DeleteProductTemplateAsync(productTemplateId, userId);
                return Ok();
            }
            catch (InvalidUserIdException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID: {UserId}", userId);
                return Unauthorized(ex.Message);
            }
            catch (ProductTemplateNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product template not found: {ProductTemplateId}", productTemplateId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedProductTemplateAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for user {UserId} to delete product template {ProductTemplateId}", userId, productTemplateId);
                return Forbid(ex.Message);
            }
            catch (ProductTemplateHasInventoryException ex)
            {
                _logger.LogWarning(ex, "Cannot delete product template with inventory: {ProductTemplateId}", productTemplateId);
                return UnprocessableEntity(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Database error while deleting product template {ProductTemplateId}", productTemplateId);
                return StatusCode(500, "An error occurred while deleting the product template.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting product template {ProductTemplateId}", productTemplateId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
