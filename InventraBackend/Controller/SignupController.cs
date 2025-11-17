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
        /////////////////////////////Signup Method Added Below////////////////////////////
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

                // ✅ Check if email already exists
                var emailCheckCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @e", connection);
                emailCheckCmd.Parameters.AddWithValue("@e", request.Email);
                var emailCount = Convert.ToInt32(await emailCheckCmd.ExecuteScalarAsync());
                if (emailCount > 0)
                    return BadRequest("Email already registered.");

                // ✅ Check if username already exists
                var usernameCheckCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @u", connection);
                usernameCheckCmd.Parameters.AddWithValue("@u", request.Username);
                var usernameCount = Convert.ToInt32(await usernameCheckCmd.ExecuteScalarAsync());
                if (usernameCount > 0)
                    return BadRequest("Username already taken.");

                // ✅ Insert new user
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

                // ✅ Send verification email
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
        ////////////////////////////Email Verification Method Added Below////////////////////////////
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
        ////////////////////////////Login Method Added Below////////////////////////////
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            try
            {
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new MySqlCommand(@"
                    SELECT email, username, password, IsVerified, TokenExpires
                    FROM users
                    WHERE username = @u;
                ", connection);
                cmd.Parameters.AddWithValue("@u", request.Username);

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return Unauthorized("Invalid username or password.");

                int emailIndex = reader.GetOrdinal("email");
                int passwordIndex = reader.GetOrdinal("password");
                int verifiedIndex = reader.GetOrdinal("IsVerified");
                int expiresIndex = reader.GetOrdinal("TokenExpires");

                string email = reader.GetString(emailIndex);
                string hashedPassword = reader.GetString(passwordIndex);
                bool isVerified = reader.GetBoolean(verifiedIndex);
                DateTime? tokenExpires = reader.IsDBNull(expiresIndex)
                    ? null
                    : reader.GetDateTime(expiresIndex);

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, hashedPassword);
                if (!isPasswordValid)
                    return Unauthorized("Invalid username or password.");

                // 🔒 User is not verified
                if (!isVerified)
                {
                    // Check if their old token expired
                    if (tokenExpires == null || tokenExpires < DateTime.UtcNow)
                    {
                        await reader.CloseAsync();

                        // Generate a new token
                        var newToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                        var newExpiry = DateTime.UtcNow.AddHours(24);

                        var updateCmd = new MySqlCommand(@"
                            UPDATE users
                            SET VerificationToken = @t, TokenExpires = @x
                            WHERE username = @u;
                        ", connection);
                        updateCmd.Parameters.AddWithValue("@t", newToken);
                        updateCmd.Parameters.AddWithValue("@x", newExpiry);
                        updateCmd.Parameters.AddWithValue("@u", request.Username);
                        await updateCmd.ExecuteNonQueryAsync();

                        // Resend verification email
                        await _emailService.SendVerificationEmailAsync(email, newToken);

                        return Unauthorized("Your verification link expired. A new verification email has been sent.");
                    }

                    return Unauthorized("Please verify your email before logging in.");


                }
                HttpContext.Session.SetString("Username", request.Username);
                HttpContext.Session.SetString("Email", email);
                // ✅ Verified user
                return Ok(new
                {
                    message = "Login successful!",
                    username = request.Username
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //////////////////////////////Logout Method Added Below////////////////////////////
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }

        //////////////////////////////Get Profile Method Added Below////////////////////////////
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not logged in.");

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new MySqlCommand("SELECT username, email, IsVerified FROM users WHERE username = @u", connection);
            cmd.Parameters.AddWithValue("@u", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return NotFound("User not found.");

            return Ok(new
            {
                Username = reader["username"].ToString(),
                Email = reader["email"].ToString(),
                Verified = Convert.ToBoolean(reader["IsVerified"])
            });
        }
    }
}