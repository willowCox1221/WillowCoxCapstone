using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using InventraBackend.Services;
using InventraBackend.Models;
using System.Security.Cryptography;




namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly string _connectionString;

        public UsersController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is required.");

            try
            {
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new MySqlCommand(@"
            SELECT username, email, createdAt, IsVerified
            FROM users
            WHERE username = @u;
        ", connection);

                cmd.Parameters.AddWithValue("@u", username);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return NotFound("User not found.");

                var usernameOrdinal = reader.GetOrdinal("username");
                var emailOrdinal = reader.GetOrdinal("email");
                var createdAtOrdinal = reader.GetOrdinal("createdAt");
                var isVerifiedOrdinal = reader.GetOrdinal("IsVerified");

                var user = new
                {
                    Username = reader.GetString(usernameOrdinal),
                    Email = reader.IsDBNull(emailOrdinal) ? null : reader.GetString(emailOrdinal),
                    CreatedAt = reader.IsDBNull(createdAtOrdinal)
                        ? null
                        : reader.GetDateTime(createdAtOrdinal).ToString("yyyy-MM-dd"),
                    IsVerified = !reader.IsDBNull(isVerifiedOrdinal) && reader.GetBoolean(isVerifiedOrdinal)
                };

                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Error retrieving user profile.");
            }
        }
    }
}