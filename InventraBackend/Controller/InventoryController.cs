using Microsoft.AspNetCore.Mvc;
using InventraBackend.Services;

namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }
/////////////////////////POSTS/////////////////////////////////////
        [HttpPost("add")]
        public async Task<IActionResult> AddToInventory([FromBody] dynamic body)
        {
            string userId = body.userId;
            string code = body.code;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
                return BadRequest(new { success = false, message = "Missing data" });

            await _inventoryService.AddItemAsync(userId, code);
            return Ok(new { success = true });
        }
////////////////////////GETS//////////////////////////////////////
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetInventory(string userId)
        {
            var inventory = await _inventoryService.GetInventoryAsync(userId);
            return Ok(inventory);
        }
    }
}