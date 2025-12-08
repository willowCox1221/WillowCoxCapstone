using InventraBackend.Models;

namespace InventraBackend.Services
{
    public class InventoryServiceAdapter : IToolService
    {
        private readonly InventoryService _inventoryService;

        public InventoryServiceAdapter(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task AddItem(InventoryItem item)
        {
            await _inventoryService.AddItem(item);
        }

        public async Task<List<InventoryItem>> GetAllItems()
        {
            return await _inventoryService.GetAllItems();
        }
    }
}