using System.Security.Cryptography;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using BCrypt.Net;

namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string email, [FromForm] string password)
        {
            // Connect to MySQL
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Check if email exists
            var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @e", connection);
            checkCmd.Parameters.AddWithValue("@e", email);
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (exists)
                return BadRequest("Email already registered.");

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Generate verification token
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var expiry = DateTime.UtcNow.AddHours(24);

            // Insert user
            var cmd = new MySqlCommand(
                "INSERT INTO users (email, password_hash, is_verified, verification_token, token_expires) " +
                "VALUES (@e, @p, FALSE, @t, @x)",
                connection
            );
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@p", passwordHash);
            cmd.Parameters.AddWithValue("@t", token);
            cmd.Parameters.AddWithValue("@x", expiry);
            await cmd.ExecuteNonQueryAsync();

            await connection.CloseAsync();

            // Send verification email
            var verifyUrl = $"https://localhost:5001/api/auth/verify?token={token}";
            await SendVerificationEmail(email, verifyUrl);

            return Ok("Verification email sent! Please check your inbox.");
        }

        private async Task SendVerificationEmail(string email, string verifyUrl)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Inventra Team", "bugaroo5455@gmail.com"));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Verify your Inventra account";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <h2>Welcome to Inventra!</h2>
                    <p>Click below to verify your account:</p>
                    <a href='{verifyUrl}'>Verify Email</a>
                    <p>This link expires in 24 hours.</p>"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("bugaroo5455@gmail.com", "tgcmzmuejlfbdgjr"); // Gmail App Password
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid token.");

            using var connection = new MySqlConnection("Server=localhost;Database=inventra;User ID=root;Password=yourpassword;");
            await connection.OpenAsync();

            // Check if token exists and is still valid
            var checkCmd = new MySqlCommand(
                "SELECT email, TokenExpires FROM users WHERE VerificationToken = @t", connection);
            checkCmd.Parameters.AddWithValue("@t", token);

            using var reader = await checkCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return BadRequest("Invalid or expired token.");

            var expiry = reader.GetDateTime(reader.GetOrdinal("TokenExpires"));
            var email = reader.GetString(reader.GetOrdinal("email"));
            await reader.CloseAsync();

            // Check if token has expired
            if (expiry < DateTime.UtcNow)
                return BadRequest("Verification link has expired.");

            // Mark account as verified
            var updateCmd = new MySqlCommand(
                "UPDATE users SET IsVerified = TRUE, VerificationToken = NULL, TokenExpires = NULL WHERE email = @e", connection);
            updateCmd.Parameters.AddWithValue("@e", email);

            await updateCmd.ExecuteNonQueryAsync();

            return Ok("Email verified successfully! You can now log in.");
        }
    }
}