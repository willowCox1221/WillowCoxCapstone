using InventraBackend.Data;
using InventraBackend.Models;
using Microsoft.EntityFrameworkCore;

public class ToolService
{
    private readonly AppDbContext _context;

    public ToolService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddTool(Tool tool)
    {
        _context.Tools.Add(tool);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Tool>> GetAllTools()
    {
        return await _context.Tools.ToListAsync();
    }

    public async Task<Tool?> GetToolById(int id)
    {
        return await _context.Tools.FindAsync(id);
    }

    public async Task DeleteTool(int id)
    {
        var tool = await _context.Tools.FindAsync(id);
        if (tool != null)
        {
            _context.Tools.Remove(tool);
            await _context.SaveChangesAsync();
        }
    }
}