using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xWarehouseNet.Db.Models;

[Table("Organizations", Schema = "app")]
public class Organization
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }

    public ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
    public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public ICollection<Location> Locations { get; set; } = new List<Location>();
    public ICollection<ProductTemplate> ProductTemplates { get; set; } = new List<ProductTemplate>();
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
