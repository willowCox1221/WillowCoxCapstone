using InventraBackend.Models;
using MongoDB.Driver;

namespace InventraBackend.Services
{
    public class InventoryService
    {
        private readonly IMongoCollection<UserInventory> _inventoryCollection;

        public InventoryService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase("inventra");
            _inventoryCollection = database.GetCollection<UserInventory>("inventories");
        }

        public async Task AddItemAsync(string userId, string code)
        {
            var filter = Builders<UserInventory>.Filter.Eq(x => x.UserId, userId);
            var update = Builders<UserInventory>.Update.Push(x => x.Items, new InventoryItem { Code = code });
            await _inventoryCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        }

        public async Task<UserInventory?> GetInventoryAsync(string userId)
        {
            return await _inventoryCollection.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        }
    }
}