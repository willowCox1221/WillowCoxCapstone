using System;

namespace InventraBackend.Models
{
    public class InventoryItem
    {
        public string Code { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}