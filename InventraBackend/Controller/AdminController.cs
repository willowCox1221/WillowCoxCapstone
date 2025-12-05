using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using InventraBackend.Models;

namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly string _connectionString;

        public AdminController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // ---------------------------------------------------------------------
        // 🔐 Helper: Check if current user is admin
        // ---------------------------------------------------------------------
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "admin";
        }

        // ---------------------------------------------------------------------
        // 👤 1. Get All Users
        // ---------------------------------------------------------------------
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            if (!IsAdmin()) return Unauthorized("Admin access required.");

            var users = new List<User>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("SELECT id, username, email, role, createdAt FROM users", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int idIndex = reader.GetOrdinal("id");
                int usernameIndex = reader.GetOrdinal("username");
                int emailIndex = reader.GetOrdinal("email");
                int roleIndex = reader.GetOrdinal("role");
                int createdAtIndex = reader.GetOrdinal("createdAt");

                users.Add(new User
                {
                    id = reader.GetInt32(idIndex),
                    username = reader.GetString(usernameIndex),
                    email = reader.GetString(emailIndex),
                    role = reader.GetString(roleIndex),
                    createdAt = reader.GetDateTime(createdAtIndex)
                });
            
            }

            return Ok(users);
        }

        // ---------------------------------------------------------------------
        // 🗑️ 2. Delete User
        // ---------------------------------------------------------------------
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin()) return Unauthorized("Admin access required.");

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("DELETE FROM users WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            int result = await cmd.ExecuteNonQueryAsync();

            return result > 0 ? Ok("User deleted.") : NotFound("User not found.");
        }

        // ---------------------------------------------------------------------
        // ⭐ 3. Promote User to Admin
        // ---------------------------------------------------------------------
        [HttpPut("promote/{id}")]
        public async Task<IActionResult> PromoteUser(int id)
        {
            if (!IsAdmin()) return Unauthorized("Admin access required.");

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("UPDATE users SET role='admin' WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
            return Ok("User promoted to admin.");
        }

        // ---------------------------------------------------------------------
        // 🔽 4. Demote Admin to User
        // ---------------------------------------------------------------------
        [HttpPut("demote/{id}")]
        public async Task<IActionResult> DemoteUser(int id)
        {
            if (!IsAdmin()) return Unauthorized("Admin access required.");

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("UPDATE users SET role='user' WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
            return Ok("Admin demoted to user.");
        }
    }
}