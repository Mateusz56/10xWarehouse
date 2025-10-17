using System.ComponentModel.DataAnnotations;
using _10xWarehouseNet.Dtos.OrganizationDtos;

namespace _10xWarehouseNet.Dtos;

public record RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public bool CreateOrganization { get; set; } = false;

    [StringLength(100, MinimumLength = 2)]
    public string? OrganizationName { get; set; }
}

public record RegisterResponseDto
{
    public UserProfileDto User { get; set; } = new();
    public OrganizationDto? Organization { get; set; }
}

public record UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public record UpdateUserProfileRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;
}

public record ChangePasswordRequestDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;
}