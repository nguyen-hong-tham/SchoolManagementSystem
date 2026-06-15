using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassService.Data;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Repositories;

public class HomeroomAssignmentRepository : IHomeroomAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public HomeroomAssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HomeroomAssignment?> GetActiveAssignmentByClassAsync(
        Guid classId,
        string schoolYear
    )
    {
        return await _context.HomeroomAssignments.FirstOrDefaultAsync(x =>
            x.ClassId == classId && x.SchoolYear == schoolYear
        );
    }

    public async Task<List<HomeroomAssignment>> GetHistoryByTeacherAsync(Guid teacherId)
    {
        return await _context
            .HomeroomAssignments.Where(h => h.TeacherId == teacherId)
            .OrderByDescending(h => h.AssignedDate)
            .ToListAsync();
    }

    public async Task AddAsync(HomeroomAssignment entity)
    {
        await _context.HomeroomAssignments.AddAsync(entity);
    }

    public void Update(HomeroomAssignment entity)
    {
        _context.HomeroomAssignments.Update(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
