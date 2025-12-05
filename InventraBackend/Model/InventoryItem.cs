using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("inventory")]
public class InventoryItem
{
    [Key]
    public int id { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
}