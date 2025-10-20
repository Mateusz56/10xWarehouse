using _10xWarehouseNet.Db.Enums;

namespace _10xWarehouseNet.Dtos.OrganizationDtos;

using System.ComponentModel.DataAnnotations;

public record OrganizationDto(Guid Id, string Name);

public record OrganizationMemberDto(Guid? UserId, string Email, UserRole Role, InvitationStatus Status)
{
    public string? UserDisplayName { get; init; }
}

public record InvitationDto(Guid Id, Guid InvitedUserId, UserRole Role, InvitationStatus Status)
{
    public string? InvitedUserEmail { get; init; }
    public string? InvitedUserDisplayName { get; init; }
}

public record InvitationWithUserDto(Guid Id, Guid InvitedUserId, string InvitedUserEmail, string InvitedUserDisplayName, UserRole Role, InvitationStatus Status);

public record UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<MembershipDto> Memberships { get; set; } = new();
}

public record UserSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public record MembershipDto
{
    public string OrganizationId { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public record CreateOrganizationRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
}

public record UpdateOrganizationRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
}

public record CreateInvitationRequestDto
{
    [Required]
    public Guid InvitedUserId { get; set; }
    
    [Required]
    public UserRole Role { get; set; }
}

public record UserInvitationDto
{
    public Guid Id { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime InvitedAt { get; set; }
}

// Command/Request Models

public record CreateOrganizationCommand(string Name);

public record UpdateOrganizationCommand(Guid OrganizationId, string Name);

public record CreateInvitationCommand(Guid InvitedUserId, UserRole Role);
