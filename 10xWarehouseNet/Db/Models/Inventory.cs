using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xWarehouseNet.Db.Models;

[Table("Inventory", Schema = "app")]
public class Inventory
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; }

    public Guid ProductTemplateId { get; set; }
    public ProductTemplate ProductTemplate { get; set; }

    public Guid LocationId { get; set; }
    public Location Location { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Quantity { get; set; } = 0;
}
