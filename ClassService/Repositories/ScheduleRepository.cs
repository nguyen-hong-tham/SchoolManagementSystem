using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassService.Data;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public ScheduleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Schedule>> GetScheduleByClassAsync(Guid classId, string schoolYear)
    {
        return await _context.Schedules
            .Where(x => x.ClassId == classId && x.SchoolYear == schoolYear)
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetScheduleByTeacherAsync(Guid teacherId, string schoolYear)
    {
        return await _context.Schedules
            .Where(x => x.TeacherId == teacherId && x.SchoolYear == schoolYear)
            .ToListAsync();
    }

    public async Task<Schedule?> CheckTeacherCollisionAsync(Guid teacherId, int dayOfWeek, int period, string schoolYear)
    {
        return await _context.Schedules
            .FirstOrDefaultAsync(x => x.TeacherId == teacherId && x.DayOfWeek == dayOfWeek && x.Period == period && x.SchoolYear == schoolYear);
    }

    public async Task<Schedule?> CheckRoomCollisionAsync(string room, int dayOfWeek, int period, string schoolYear)
    {
        return await _context.Schedules
            .FirstOrDefaultAsync(x => x.Room == room && x.DayOfWeek == dayOfWeek && x.Period == period && x.SchoolYear == schoolYear);
    }

    public async Task<Schedule?> CheckClassCollisionAsync(Guid classId, int dayOfWeek, int period, string schoolYear)
    {
        return await _context.Schedules
            .FirstOrDefaultAsync(x => x.ClassId == classId && x.DayOfWeek == dayOfWeek && x.Period == period && x.SchoolYear == schoolYear);
    }

    public async Task AddAsync(Schedule entity)
    {
        await _context.Schedules.AddAsync(entity);
    }

    public void Delete(Schedule entity)
    {
        _context.Schedules.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
