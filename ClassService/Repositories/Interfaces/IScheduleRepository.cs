using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassService.Entities;

namespace ClassService.Repositories.Interfaces;

public interface IScheduleRepository
{
    Task<List<Schedule>> GetScheduleByClassAsync(Guid classId, string schoolYear);
    Task<List<Schedule>> GetScheduleByTeacherAsync(Guid teacherId, string schoolYear);
    
    Task<Schedule?> CheckTeacherCollisionAsync(Guid teacherId, int dayOfWeek, int period, string schoolYear);
    Task<Schedule?> CheckRoomCollisionAsync(string room, int dayOfWeek, int period, string schoolYear);
    Task<Schedule?> CheckClassCollisionAsync(Guid classId, int dayOfWeek, int period, string schoolYear);

    Task AddAsync(Schedule entity);
    void Delete(Schedule entity);
    Task SaveChangesAsync();
}
