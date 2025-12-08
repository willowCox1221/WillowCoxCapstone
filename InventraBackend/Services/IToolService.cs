using InventraBackend.Models;

namespace InventraBackend.Services
{
    public interface IToolService
    {
        Task AddItem(InventoryItem item);
        Task<List<InventoryItem>> GetAllItems();
    }
}