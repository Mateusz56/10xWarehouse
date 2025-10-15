using System.ComponentModel.DataAnnotations;

namespace _10xWarehouseNet.Dtos;

// Data Transfer Objects

public record ProductTemplateDto(Guid Id, string Name, string? Barcode, string? Description, decimal? LowStockThreshold);

// Request DTOs

public record CreateProductTemplateRequestDto
{
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Barcode { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Low stock threshold must be non-negative")]
    public decimal? LowStockThreshold { get; set; } = 0;
}

// Command Models

public record CreateProductTemplateCommand(string Name, string? Barcode, string? Description, decimal LowStockThreshold);
