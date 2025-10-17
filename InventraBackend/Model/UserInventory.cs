using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace InventraBackend.Models
{
    public class UserInventory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public List<InventoryItem> Items { get; set; } = new();
    }
}