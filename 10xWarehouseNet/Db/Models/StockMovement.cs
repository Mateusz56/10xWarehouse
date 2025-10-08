using _10xWarehouseNet.Db.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xWarehouseNet.Db.Models;

[Table("StockMovements", Schema = "app")]
public class StockMovement
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; }

    public Guid ProductTemplateId { get; set; }
    public ProductTemplate ProductTemplate { get; set; }

    [Required]
    public MovementType MovementType { get; set; }

    public Guid? FromLocationId { get; set; }
    public Location? FromLocation { get; set; }

    public Guid? ToLocationId { get; set; }
    public Location? ToLocation { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Delta { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total { get; set; }

    public Guid? UserId { get; set; }
}
