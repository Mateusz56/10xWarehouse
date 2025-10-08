using _10xWarehouseNet.Db.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xWarehouseNet.Db.Models;

[Table("Invitations", Schema = "app")]
public class Invitation
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; }

    public Guid? UserId { get; set; }
    
    [Required]
    public UserRole Role { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
}
