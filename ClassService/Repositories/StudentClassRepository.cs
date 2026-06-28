using ClassService.Data;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Repositories;

public class StudentClassRepository : IStudentClassRepository
{
    private readonly ApplicationDbContext _context;

    public StudentClassRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentClass>> GetStudentsByClassIdAsync(Guid classId, bool onlyCurrent = false)
    {
        var query = _context.StudentClasses.Where(x => x.ClassId == classId);
        if (onlyCurrent)
        {
            query = query.Where(x => x.IsCurrent);
        }
        else
        {
            query = query.Where(x => x.PromotionStatus != "Transferred");
        }
        return await query.ToListAsync();
    }

    public async Task<StudentClass?> GetCurrentStudentClassAsync(Guid studentId)
    {
        return await _context.StudentClasses.FirstOrDefaultAsync(x =>
            x.StudentId == studentId && x.IsCurrent
        );
    }

    public async Task AddAsync(StudentClass studentClass)
    {
        await _context.StudentClasses.AddAsync(studentClass);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
