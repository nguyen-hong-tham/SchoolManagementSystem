using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassService.Data;
using ClassService.DTOs.Schedules;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using ClassService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Services;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IClassRepository _classRepository;
    private readonly ApplicationDbContext _dbContext;

    public ScheduleService(
        IScheduleRepository scheduleRepository,
        IClassRepository classRepository,
        ApplicationDbContext dbContext)
    {
        _scheduleRepository = scheduleRepository;
        _classRepository = classRepository;
        _dbContext = dbContext;
    }

    public async Task<ScheduleResponseDto> CreateScheduleAsync(CreateScheduleDto dto)
    {
        var targetClass = await _classRepository.GetByIdAsync(dto.ClassId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {dto.ClassId}");
        }

        // 1. Kiểm tra va chạm lịch dạy của Giáo viên
        var teacherCollision = await _scheduleRepository.CheckTeacherCollisionAsync(
            dto.TeacherId, dto.DayOfWeek, dto.Period, dto.SchoolYear);
        if (teacherCollision != null)
        {
            throw new InvalidOperationException("Giáo viên này đã có lịch dạy ở lớp học khác vào thời gian này.");
        }

        // 2. Kiểm tra va chạm Phòng học
        var roomCollision = await _scheduleRepository.CheckRoomCollisionAsync(
            dto.Room, dto.DayOfWeek, dto.Period, dto.SchoolYear);
        if (roomCollision != null)
        {
            throw new InvalidOperationException("Phòng học này đã được sử dụng bởi lớp khác vào thời gian này.");
        }

        // 3. Kiểm tra va chạm lịch của chính Lớp học đó
        var classCollision = await _scheduleRepository.CheckClassCollisionAsync(
            dto.ClassId, dto.DayOfWeek, dto.Period, dto.SchoolYear);
        if (classCollision != null)
        {
            throw new InvalidOperationException("Lớp học này đã có môn học khác được xếp lịch vào thời gian này.");
        }

        var entity = new Schedule
        {
            Id = Guid.NewGuid(),
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId,
            TeacherId = dto.TeacherId,
            DayOfWeek = dto.DayOfWeek,
            Period = dto.Period,
            Room = dto.Room,
            SchoolYear = dto.SchoolYear
        };

        await _scheduleRepository.AddAsync(entity);
        await _scheduleRepository.SaveChangesAsync();

        return await MapToResponseDtoAsync(entity);
    }

    public async Task<IEnumerable<ScheduleResponseDto>> GetClassScheduleAsync(Guid classId, string? schoolYear)
    {
        var targetClass = await _classRepository.GetByIdAsync(classId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {classId}");
        }

        if (string.IsNullOrEmpty(schoolYear))
        {
            schoolYear = targetClass.SchoolYear;
        }

        var schedules = await _scheduleRepository.GetScheduleByClassAsync(classId, schoolYear);
        
        var teacherIds = schedules.Select(s => s.TeacherId).Distinct().ToList();
        var teachers = await _dbContext.CachedUsers.Where(u => teacherIds.Contains(u.Id)).ToListAsync();

        var subjectIds = schedules.Select(s => s.SubjectId).Distinct().ToList();
        var subjects = await _dbContext.CachedSubjects.Where(s => subjectIds.Contains(s.Id)).ToListAsync();

        return schedules.Select(entity => {
            var teacher = teachers.FirstOrDefault(t => t.Id == entity.TeacherId);
            var subject = subjects.FirstOrDefault(s => s.Id == entity.SubjectId);
            return new ScheduleResponseDto
            {
                Id = entity.Id,
                ClassId = entity.ClassId,
                SubjectId = entity.SubjectId,
                TeacherId = entity.TeacherId,
                DayOfWeek = entity.DayOfWeek,
                Period = entity.Period,
                Room = entity.Room,
                SchoolYear = entity.SchoolYear,
                TeacherName = teacher?.FullName ?? string.Empty,
                SubjectName = subject?.Name ?? string.Empty
            };
        }).ToList();
    }

    public async Task<IEnumerable<ScheduleResponseDto>> GetTeacherScheduleAsync(Guid teacherId, string? schoolYear)
    {
        if (string.IsNullOrEmpty(schoolYear))
        {
            schoolYear = "2025-2026";
        }

        var schedules = await _scheduleRepository.GetScheduleByTeacherAsync(teacherId, schoolYear);
        
        var teacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == teacherId);

        var subjectIds = schedules.Select(s => s.SubjectId).Distinct().ToList();
        var subjects = await _dbContext.CachedSubjects.Where(s => subjectIds.Contains(s.Id)).ToListAsync();

        return schedules.Select(entity => {
            var subject = subjects.FirstOrDefault(s => s.Id == entity.SubjectId);
            return new ScheduleResponseDto
            {
                Id = entity.Id,
                ClassId = entity.ClassId,
                SubjectId = entity.SubjectId,
                TeacherId = entity.TeacherId,
                DayOfWeek = entity.DayOfWeek,
                Period = entity.Period,
                Room = entity.Room,
                SchoolYear = entity.SchoolYear,
                TeacherName = teacher?.FullName ?? string.Empty,
                SubjectName = subject?.Name ?? string.Empty
            };
        }).ToList();
    }

    private async Task<ScheduleResponseDto> MapToResponseDtoAsync(Schedule entity)
    {
        var teacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == entity.TeacherId);
        var subject = await _dbContext.CachedSubjects.FirstOrDefaultAsync(s => s.Id == entity.SubjectId);

        return new ScheduleResponseDto
        {
            Id = entity.Id,
            ClassId = entity.ClassId,
            SubjectId = entity.SubjectId,
            TeacherId = entity.TeacherId,
            DayOfWeek = entity.DayOfWeek,
            Period = entity.Period,
            Room = entity.Room,
            SchoolYear = entity.SchoolYear,
            TeacherName = teacher?.FullName ?? string.Empty,
            SubjectName = subject?.Name ?? string.Empty
        };
    }
}
