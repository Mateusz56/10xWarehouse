using _10xWarehouseNet.Db.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xWarehouseNet.Db.Models;

[Table("OrganizationMembers", Schema = "app")]
public class OrganizationMember
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; }

    public Guid UserId { get; set; }

    [Required]
    public UserRole Role { get; set; }
}
