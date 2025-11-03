using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignupController : ControllerBase
    {
        private readonly string _connectionString = "server=localhost;user=root;password=Test;database=inventra;";

        [HttpPost]
        public IActionResult SignUp([FromForm] string email, [FromForm] string username, [FromForm] string password)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string query = "INSERT INTO users (email, username, password) VALUES (@Email, @Username, @Password)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.ExecuteNonQuery();

                return Ok(new { message = "User registered successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}