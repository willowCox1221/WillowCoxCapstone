using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace InventraBackend.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AdminController(IConfiguration config)
        {
            _config = config;
        }

        private MySqlConnection GetConn() =>
            new MySqlConnection(_config.GetConnectionString("DefaultConnection"));

        // GET ALL USERS
        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            if (!IsAdmin()) return Unauthorized("Not an admin");

            var list = new List<object>();
            using var conn = GetConn();
            conn.Open();

            string sql = "SELECT Username, Email, IsVerified, IsAdmin FROM users";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new
                {
                    Username = reader.GetString("Username"),
                    Email = reader.GetString("Email"),
                    IsVerified = reader.GetBoolean("IsVerified"),
                    IsAdmin = reader.GetBoolean("IsAdmin")
                });
            }
            return Ok(list);
        }

        // PROMOTE USER
        [HttpPut("promote/{username}")]
        public IActionResult Promote(string username)
        {
            if (!IsAdmin()) return Unauthorized("Not an admin");

            using var conn = GetConn();
            conn.Open();

            string sql = "UPDATE users SET IsAdmin = 1 WHERE Username = @u";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.ExecuteNonQuery();

            return Ok("User promoted.");
        }

        // DELETE USER
        [HttpDelete("delete/{username}")]
        public IActionResult Delete(string username)
        {
            if (!IsAdmin()) return Unauthorized("Not an admin");

            using var conn = GetConn();
            conn.Open();

            string sql = "DELETE FROM users WHERE Username = @u";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.ExecuteNonQuery();

            return Ok("User deleted.");
        }

        // RESET PASSWORD
        [HttpPut("reset/{username}")]
        public IActionResult ResetPassword(string username)
        {
            if (!IsAdmin()) return Unauthorized("Not an admin");

            using var conn = GetConn();
            conn.Open();

            string newHash = BCrypt.Net.BCrypt.HashPassword("temporary123");

            string sql = "UPDATE users SET Password = @p WHERE Username = @u";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", newHash);
            cmd.ExecuteNonQuery();

            return Ok("Password reset.");
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("IsAdmin") == "true";
        }
    }
}