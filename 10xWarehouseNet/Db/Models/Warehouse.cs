using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xWarehouseNet.Db.Models;

[Table("Warehouses", Schema = "app")]
public class Warehouse
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; }

    [Required]
    public string Name { get; set; }
    
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
