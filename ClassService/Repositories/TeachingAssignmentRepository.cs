using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassService.Data;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Repositories;

public class TeachingAssignmentRepository : ITeachingAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public TeachingAssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeachingAssignment>> GetAssignmentsByClassAsync(
        Guid classId,
        string schoolYear
    )
    {
        return await _context
            .TeachingAssignments.Where(x => x.ClassId == classId && x.SchoolYear == schoolYear)
            .ToListAsync();
    }

    public async Task<List<TeachingAssignment>> GetAssignmentsByTeacherAsync(
        Guid teacherId,
        string schoolYear
    )
    {
        return await _context
            .TeachingAssignments.Where(x => x.TeacherId == teacherId && x.SchoolYear == schoolYear)
            .ToListAsync();
    }

    public async Task<TeachingAssignment?> GetAssignmentAsync(
        Guid classId,
        Guid subjectId,
        string schoolYear
    )
    {
        return await _context.TeachingAssignments.FirstOrDefaultAsync(x =>
            x.ClassId == classId && x.SubjectId == subjectId && x.SchoolYear == schoolYear
        );
    }

    public async Task AddAsync(TeachingAssignment entity)
    {
        await _context.TeachingAssignments.AddAsync(entity);
    }

    public void Update(TeachingAssignment entity)
    {
        _context.TeachingAssignments.Update(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
