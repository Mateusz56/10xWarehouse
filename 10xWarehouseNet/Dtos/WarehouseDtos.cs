using System.ComponentModel.DataAnnotations;

namespace _10xWarehouseNet.Dtos;

// Data Transfer Objects

public record WarehouseDto(Guid Id, string Name, Guid OrganizationId);

public record WarehouseWithLocationsDto(Guid Id, string Name, Guid OrganizationId, List<LocationDto> Locations);

public record LocationDto(Guid Id, string Name, string? Description, Guid WarehouseId);

// Request DTOs

public record CreateWarehouseRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public Guid OrganizationId { get; set; }
}

public record UpdateWarehouseRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

// Location Request DTOs

public record CreateLocationRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public Guid WarehouseId { get; set; }
}

public record UpdateLocationRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
}

// Command Models (Legacy - keeping for backward compatibility)

public record CreateWarehouseCommand(string Name);

public record UpdateWarehouseCommand(string Name);

public record CreateLocationCommand(string Name, string? Description);
