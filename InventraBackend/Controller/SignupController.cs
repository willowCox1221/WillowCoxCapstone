using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using InventraBackend.Services;
using System.Security.Cryptography;

namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignupController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly MySqlConnection _connection;

        public SignupController(IConfiguration config, EmailService emailService)
        {
            _emailService = emailService;
            _connection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        // Generate a secure random verification token
        private static string GenerateVerificationToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(tokenBytes);
        }

        [HttpPost]
        public async Task<IActionResult> Signup([FromForm] string email, [FromForm] string username, [FromForm] string password)
        {
            var token = GenerateVerificationToken();

            try
            {
                await _connection.OpenAsync();

                var cmd = new MySqlCommand(@"
                    INSERT INTO users (email, username, password, VerificationToken, IsVerified)
                    VALUES (@e, @u, @p, @t, FALSE);
                ", _connection);

                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password); // ⚠️ You should hash this later!
                cmd.Parameters.AddWithValue("@t", token);

                await cmd.ExecuteNonQueryAsync();

                // Send email verification
                await _emailService.SendVerificationEmailAsync(email, token);

                return Ok(new
                {
                    message = "Signup successful! Please check your email to verify your account."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Signup failed: {ex.Message}" });
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                await _connection.OpenAsync();

                var cmd = new MySqlCommand(@"
                    UPDATE users 
                    SET IsVerified = TRUE 
                    WHERE VerificationToken = @t AND IsVerified = FALSE;
                ", _connection);

                cmd.Parameters.AddWithValue("@t", token);
                int rows = await cmd.ExecuteNonQueryAsync();

                if (rows > 0)
                    return Ok("✅ Email verified successfully!");
                else
                    return BadRequest("❌ Invalid or already verified token.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Verification failed: {ex.Message}" });
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
    }
}