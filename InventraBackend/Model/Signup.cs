namespace InventraBackend.Models
{
    public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? VerificationToken { get; set; }
    public bool IsVerified { get; set; }
}
}