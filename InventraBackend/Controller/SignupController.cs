using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using InventraBackend.Services;
using InventraBackend.Models;
using System.Security.Cryptography;

namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignupController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly string _connectionString;

        public SignupController(IConfiguration config, EmailService emailService)
        {
            _emailService = emailService;
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // Generate a secure random verification token
        private static string GenerateVerificationToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(tokenBytes);
        }

        // ✅ Updated to use [FromBody] model binding for JSON
        [HttpPost]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("All fields are required.");
            }

            var token = GenerateVerificationToken();
            var tokenExpires = DateTime.UtcNow.AddHours(24);
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            try
            {
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Check if email already exists
                var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @e", connection);
                checkCmd.Parameters.AddWithValue("@e", request.Email);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                if (count > 0)
                    return BadRequest("Email already registered.");

                var cmd = new MySqlCommand(@"
                    INSERT INTO users (email, username, password, VerificationToken, TokenExpires, IsVerified)
                    VALUES (@e, @u, @p, @t, @x, FALSE);
                ", connection);

                cmd.Parameters.AddWithValue("@e", request.Email);
                cmd.Parameters.AddWithValue("@u", request.Username);
                cmd.Parameters.AddWithValue("@p", hashedPassword);
                cmd.Parameters.AddWithValue("@t", token);
                cmd.Parameters.AddWithValue("@x", tokenExpires);

                await cmd.ExecuteNonQueryAsync();

                // Send verification email
                // var verifyUrl = $"https://localhost:5001/api/signup/verify?token={token}";
                await _emailService.SendVerificationEmailAsync(request.Email, token);

                return Ok(new
                {
                    message = "Signup successful! Please check your email to verify your account."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Signup failed: {ex.Message}" });
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid token.");

            try
            {
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Validate token
                var selectCmd = new MySqlCommand(@"
                    SELECT email, TokenExpires 
                    FROM users 
                    WHERE VerificationToken = @t AND IsVerified = FALSE;
                ", connection);
                selectCmd.Parameters.AddWithValue("@t", token);

                using var reader = await selectCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return BadRequest("Invalid or already verified token.");

                var expiry = reader.GetDateTime(reader.GetOrdinal("TokenExpires"));
                var email = reader.GetString(reader.GetOrdinal("email"));
                await reader.CloseAsync();

                if (expiry < DateTime.UtcNow)
                    return BadRequest("Verification link has expired.");

                // Mark verified
                var updateCmd = new MySqlCommand(@"
                    UPDATE users 
                    SET IsVerified = TRUE, VerificationToken = NULL, TokenExpires = NULL 
                    WHERE email = @e;
                ", connection);
                updateCmd.Parameters.AddWithValue("@e", email);

                await updateCmd.ExecuteNonQueryAsync();

                return Ok("✅ Email verified successfully! You can now log in.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Verification failed: {ex.Message}" });
            }
        }
    }
}