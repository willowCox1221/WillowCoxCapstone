using Microsoft.AspNetCore.Mvc;
using InventraBackend.Models;
using MongoDB.Driver;

namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToolsController : ControllerBase
    {
        private readonly IMongoCollection<Tool> _tools;

        public ToolsController(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:InventoryDB"]);
            _tools = database.GetCollection<Tool>("Tools");
        }

        // POST: /api/tools/add
        [HttpPost("add")]
        public async Task<IActionResult> AddTool([FromBody] Tool tool)
        {
            if (tool == null || string.IsNullOrEmpty(tool.Name))
                return BadRequest(new { message = "Invalid tool data" });

            await _tools.InsertOneAsync(tool);
            return Ok(new { message = "Tool added successfully" });
        }

        // GET: /api/tools
        [HttpGet]
        public async Task<IActionResult> GetAllTools()
        {
            var tools = await _tools.Find(_ => true).ToListAsync();
            return Ok(tools);
        }
    }
}