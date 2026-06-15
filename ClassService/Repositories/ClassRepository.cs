using ClassService.Data;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Repositories;

public class ClassRepository : IClassRepository
{
    private readonly ApplicationDbContext _context;

    public ClassRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Class>> GetAllAsync()
    {
        return await _context.Classes.ToListAsync();
    }

    public async Task<Class?> GetByIdAsync(Guid id)
    {
        return await _context.Classes.FindAsync(id);
    }

    public async Task AddAsync(Class entity)
    {
        await _context.Classes.AddAsync(entity);
    }

    public void Update(Class entity)
    {
        _context.Classes.Update(entity);
    }

    public void Delete(Class entity)
    {
        _context.Classes.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
