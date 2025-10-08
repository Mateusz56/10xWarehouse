using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xWarehouseNet.Db.Models;

[Table("ProductTemplates", Schema = "app")]
public class ProductTemplate
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; }

    [Required]
    public string Name { get; set; }

    public string? Barcode { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? LowStockThreshold { get; set; } = 0;
}
