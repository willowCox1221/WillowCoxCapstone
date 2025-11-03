using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using InventraBackend.Models;

namespace InventraBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignupController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SignupController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult Register([FromForm] string username, [FromForm] string email, [FromForm] string password)
        {
            try
            {
                string connStr = _config.GetConnectionString("DefaultConnection")!;
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = "INSERT INTO users (username, email, password) VALUES (@username, @email, @password)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password); // Hash this in production!
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Signup successful!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}