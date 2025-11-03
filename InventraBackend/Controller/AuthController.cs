using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using InventraBackend.Services;
using InventraBackend.Models;
using System.Security.Cryptography;



[ApiController]
[Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MySqlConnection _connection;

        public AuthController(IConfiguration config)
        {
            _connection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        }

    [HttpGet("verify")]
    public IActionResult VerifyEmail(string token)
    {
        _connection.Open();
        var cmd = new MySqlCommand("UPDATE users SET IsVerified = TRUE WHERE VerificationToken = @token", _connection);
        cmd.Parameters.AddWithValue("@token", token);

        int rows = cmd.ExecuteNonQuery();
        _connection.Close();

        if (rows > 0)
            return Ok("✅ Email verified successfully!");
        else
            return BadRequest("Invalid or expired token.");
    }
}