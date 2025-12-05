using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public class User
{
    [Key]
    public int id { get; set; }
    public string username { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public string VerificationToken { get; set; }
    public bool IsVerified { get; set; }
    public DateTime TokenExpires { get; set; }
    public DateTime createdAt { get; set; }
    public string role { get; set; }
    public bool IsAdmin { get; set; }
}