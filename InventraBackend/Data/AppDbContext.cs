using InventraBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace InventraBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tool> Tools { get; set; }
    }
}