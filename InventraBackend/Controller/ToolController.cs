using Microsoft.AspNetCore.Mvc;
using InventraBackend.Models;
using InventraBackend.Services;

namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToolsController : ControllerBase
    {
        private readonly ToolService _toolService;

        public ToolsController(ToolService toolService)
        {
            _toolService = toolService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddTool([FromBody] Tool tool)
        {
            if (tool == null || string.IsNullOrEmpty(tool.Name))
                return BadRequest(new { message = "Invalid tool data" });

            await _toolService.AddToolAsync(tool);
            return Ok(new { message = "Tool added successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTools()
        {
            var tools = await _toolService.GetToolsAsync();
            return Ok(tools);
        }
    }
}