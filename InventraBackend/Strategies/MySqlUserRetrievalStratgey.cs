using MySql.Data.MySqlClient;
using InventraBackend.Strategies;

namespace InventraBackend.Services
{
    public class MySqlUserRetrievalStrategy : IUserRetrievalStrategy
    {
        private readonly string _connectionString;

        public MySqlUserRetrievalStrategy(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<object?> GetUserProfileAsync(string username)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new MySqlCommand(@"
                SELECT username, email, createdAt, IsVerified
                FROM users
                WHERE username = @u;
            ", connection);

            cmd.Parameters.AddWithValue("@u", username);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var user = new
            {
                Username = reader.GetString("username"),
                Email = reader.IsDBNull("email") ? null : reader.GetString("email"),
                CreatedAt = reader.IsDBNull("createdAt")
                    ? null
                    : reader.GetDateTime("createdAt").ToString("yyyy-MM-dd"),
                IsVerified = !reader.IsDBNull("IsVerified") && reader.GetBoolean("IsVerified")
            };

            return user;
        }
    }
}