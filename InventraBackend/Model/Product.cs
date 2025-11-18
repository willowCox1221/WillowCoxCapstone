using System;

namespace InventraBackend.Models
{
    public class Product
    {
        public string Code { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

}