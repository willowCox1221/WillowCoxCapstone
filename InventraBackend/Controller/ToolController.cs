using Microsoft.AspNetCore.Mvc;
using InventraBackend.Models;
using InventraBackend.Services;

namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToolsController : ControllerBase
    {
        private readonly IToolService _toolService;

        public ToolsController(IToolService toolService)
        {
            _toolService = toolService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddItem([FromBody] InventoryItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.Name))
                return BadRequest(new { message = "Invalid item data" });

            await _toolService.AddItem(item);

            return Ok(new { message = "Item added successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _toolService.GetAllItems();
            return Ok(items);
        }
    }
}