using MySql.Data.MySqlClient;

public class AdminService : IAdminService
{
    private readonly string _connectionString;

    public AdminService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<List<User>> GetUsers()
    {
        var users = new List<User>();

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand("SELECT id, username, email, role, createdAt FROM users", conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                id = reader.GetInt32("id"),
                username = reader.GetString("username"),
                email = reader.GetString("email"),
                role = reader.GetString("role"),
                createdAt = reader.GetDateTime("createdAt")
            });
        }

        return users;
    }

    public async Task<bool> DeleteUser(int id)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand("DELETE FROM users WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> PromoteUser(int id)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand("UPDATE users SET role='admin' WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DemoteUser(int id)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand("UPDATE users SET role='user' WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}