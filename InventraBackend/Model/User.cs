namespace InventraBackend.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? VerificationToken { get; set; }
        public DateTime? TokenExpires { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}