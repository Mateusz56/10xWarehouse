using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xWarehouseNet.Db.Models;

[Table("Locations", Schema = "app")]
public class Location
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }

    [Required]
    public string Name { get; set; }

    public string? Description { get; set; }
}
