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
    public Guid InvitedUserId { get; set; }

    public UserRole Role { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
}
