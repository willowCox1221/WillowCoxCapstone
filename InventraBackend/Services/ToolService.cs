using MongoDB.Driver;

public class ToolService
{
    private readonly IMongoCollection<Tool> _tools;

    public ToolService(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("MongoDB"));
        var db = client.GetDatabase("InventraDB");
        _tools = db.GetCollection<Tool>("Tools");
    }

    public async Task AddToolAsync(Tool tool)
    {
        await _tools.InsertOneAsync(tool);
    }

    public async Task<List<Tool>> GetAllToolsAsync()
    {
        return await _tools.Find(_ => true).ToListAsync();
    }
}