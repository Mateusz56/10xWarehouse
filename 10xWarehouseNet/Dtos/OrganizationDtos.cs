using _10xWarehouseNet.Db.Enums;

namespace _10xWarehouseNet.Dtos.OrganizationDtos;

using System.ComponentModel.DataAnnotations;

public record OrganizationDto(Guid Id, string Name);

public record OrganizationMemberDto(Guid? UserId, string Email, UserRole Role, InvitationStatus Status);

public record InvitationDto(Guid Id, Guid InvitedUserId, UserRole Role, InvitationStatus Status);

public record CreateOrganizationRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
}

// Command/Request Models

public record CreateOrganizationCommand(string Name);

public record CreateInvitationCommand(Guid InvitedUserId, UserRole Role);
