using MySql.Data.MySqlClient;
using InventraBackend.Models;

namespace InventraBackend.Services
{
    public class InventoryService
    {
        private readonly DatabaseService _db;

        public InventoryService(DatabaseService db)
        {
            _db = db;
        }

        public async Task AddItem(InventoryItem item)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string sql = @"INSERT INTO inventory (Name, Brand, Category, Description, Image) 
                            VALUES (@name, @brand, @category, @description, @image)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@brand", item.Brand);
            cmd.Parameters.AddWithValue("@category", item.Category);
            cmd.Parameters.AddWithValue("@description", item.Description);
            cmd.Parameters.AddWithValue("@image", item.Image);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> DeleteItem(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string sql = "DELETE FROM inventory WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<List<InventoryItem>> GetAllItems()
        {
            var items = new List<InventoryItem>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string sql = "SELECT * FROM inventory";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
{
            items.Add(new InventoryItem
            {
                id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Brand = reader.GetString(reader.GetOrdinal("Brand")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Image = reader.GetString(reader.GetOrdinal("Image"))
            });
}

            return items;
        }
    }
}