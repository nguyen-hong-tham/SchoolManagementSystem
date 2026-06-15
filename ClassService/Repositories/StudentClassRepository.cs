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

    public async Task<List<StudentClass>> GetStudentsByClassIdAsync(Guid classId)
    {
        return await _context.StudentClasses
            .Where(x => x.ClassId == classId && x.IsCurrent)
            .ToListAsync();
    }

    public async Task<StudentClass?> GetCurrentStudentClassAsync(Guid studentId)
    {
        return await _context.StudentClasses
            .FirstOrDefaultAsync(x =>
                x.StudentId == studentId &&
                x.IsCurrent);
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