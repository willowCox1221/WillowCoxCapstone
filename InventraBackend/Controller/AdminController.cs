using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using InventraBackend.Models;

public class AdminController : Controller
{
    private readonly IConfiguration _config;

    public AdminController(IConfiguration config)
    {
        _config = config;
    }

    [AdminOnly] // ⬅ protects this page
    public IActionResult Dashboard()
    {
        var users = new List<User>();
        var inventory = new List<Product>();

        string connectionString = _config.GetConnectionString("DefaultConnection");

        using (var conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // Load users
            var cmd1 = new MySqlCommand("SELECT id, username, email, role, created_at FROM users", conn);
            using (var reader = cmd1.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = reader.GetInt32("id").ToString(),
                        Username = reader.GetString("username"),
                        Email = reader.GetString("email"),
                        Role = reader.GetString("role"),
                        CreatedAt = reader.GetDateTime("created_at")
                    });
                }
            }

            // Load inventory
            var cmd2 = new MySqlCommand("SELECT * FROM inventory", conn);
            using (var reader = cmd2.ExecuteReader())
            {
                while (reader.Read())
                {
                    inventory.Add(new Product
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Quantity = reader.GetInt32("quantity")
                    });
                }
            }
        }

        ViewBag.Users = users;
        ViewBag.Inventory = inventory;

        return View();
    }
}