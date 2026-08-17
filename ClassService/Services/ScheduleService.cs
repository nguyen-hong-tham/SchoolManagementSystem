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
            var teacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == dto.TeacherId);
            var otherClass = await _classRepository.GetByIdAsync(teacherCollision.ClassId);
            throw new InvalidOperationException($"Giáo viên {teacher?.FullName ?? "này"} đã có lịch dạy tại lớp {otherClass?.Name ?? "khác"} vào Tiết {dto.Period} Thứ {dto.DayOfWeek}.");
        }

        // 2. Kiểm tra va chạm Phòng học
        var roomCollision = await _scheduleRepository.CheckRoomCollisionAsync(
            dto.Room, dto.DayOfWeek, dto.Period, dto.SchoolYear);
        if (roomCollision != null)
        {
            var otherClass = await _classRepository.GetByIdAsync(roomCollision.ClassId);
            throw new InvalidOperationException($"Phòng học {dto.Room} đã được sử dụng bởi lớp {otherClass?.Name ?? "khác"} vào Tiết {dto.Period} Thứ {dto.DayOfWeek}.");
        }

        // 3. Kiểm tra va chạm lịch của chính Lớp học đó
        var classCollision = await _scheduleRepository.CheckClassCollisionAsync(
            dto.ClassId, dto.DayOfWeek, dto.Period, dto.SchoolYear);
        if (classCollision != null)
        {
            var sub = await _dbContext.CachedSubjects.FirstOrDefaultAsync(s => s.Id == classCollision.SubjectId);
            throw new InvalidOperationException($"Lớp học này đã có môn {sub?.Name ?? "học khác"} vào Tiết {dto.Period} Thứ {dto.DayOfWeek}.");
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

    public async Task<ScheduleResponseDto> UpdateScheduleAsync(Guid id, UpdateScheduleDto dto)
    {
        var existing = await _scheduleRepository.GetByIdAsync(id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tiết học thời khóa biểu với ID: {id}");
        }

        var targetClass = await _classRepository.GetByIdAsync(existing.ClassId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {existing.ClassId}");
        }

        var schoolYear = string.IsNullOrEmpty(dto.SchoolYear) ? existing.SchoolYear : dto.SchoolYear;

        // 1. Kiểm tra va chạm lịch dạy của Giáo viên (loại trừ chính tiết này)
        var teacherCollision = await _scheduleRepository.CheckTeacherCollisionAsync(
            dto.TeacherId, dto.DayOfWeek, dto.Period, schoolYear, id);
        if (teacherCollision != null)
        {
            var teacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == dto.TeacherId);
            var otherClass = await _classRepository.GetByIdAsync(teacherCollision.ClassId);
            throw new InvalidOperationException($"Giáo viên {teacher?.FullName ?? "này"} đã có lịch dạy tại lớp {otherClass?.Name ?? "khác"} vào Tiết {dto.Period} Thứ {dto.DayOfWeek}.");
        }

        // 2. Kiểm tra va chạm Phòng học (loại trừ chính tiết này)
        var roomCollision = await _scheduleRepository.CheckRoomCollisionAsync(
            dto.Room, dto.DayOfWeek, dto.Period, schoolYear, id);
        if (roomCollision != null)
        {
            var otherClass = await _classRepository.GetByIdAsync(roomCollision.ClassId);
            throw new InvalidOperationException($"Phòng học {dto.Room} đã được sử dụng bởi lớp {otherClass?.Name ?? "khác"} vào Tiết {dto.Period} Thứ {dto.DayOfWeek}.");
        }

        // 3. Kiểm tra va chạm lịch của chính Lớp học đó (loại trừ chính tiết này)
        var classCollision = await _scheduleRepository.CheckClassCollisionAsync(
            existing.ClassId, dto.DayOfWeek, dto.Period, schoolYear, id);
        if (classCollision != null)
        {
            var sub = await _dbContext.CachedSubjects.FirstOrDefaultAsync(s => s.Id == classCollision.SubjectId);
            throw new InvalidOperationException($"Lớp học này đã có môn {sub?.Name ?? "học khác"} vào Tiết {dto.Period} Thứ {dto.DayOfWeek}.");
        }

        // Cập nhật thông tin
        existing.SubjectId = dto.SubjectId;
        existing.TeacherId = dto.TeacherId;
        existing.DayOfWeek = dto.DayOfWeek;
        existing.Period = dto.Period;
        existing.Room = dto.Room;
        existing.SchoolYear = schoolYear;

        await _scheduleRepository.SaveChangesAsync();

        return await MapToResponseDtoAsync(existing);
    }

    public async Task DeleteScheduleAsync(Guid id)
    {
        var existing = await _scheduleRepository.GetByIdAsync(id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tiết học với ID: {id}");
        }

        _scheduleRepository.Delete(existing);
        await _scheduleRepository.SaveChangesAsync();
    }

    public async Task ClearClassScheduleAsync(Guid classId, string? schoolYear)
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

        await _scheduleRepository.ClearClassScheduleAsync(classId, schoolYear);
        await _scheduleRepository.SaveChangesAsync();
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
                ClassName = targetClass.Name,
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

        var classIds = schedules.Select(s => s.ClassId).Distinct().ToList();
        var classes = await _classRepository.GetAllAsync();

        return schedules.Select(entity => {
            var subject = subjects.FirstOrDefault(s => s.Id == entity.SubjectId);
            var targetClass = classes.FirstOrDefault(c => c.Id == entity.ClassId);
            return new ScheduleResponseDto
            {
                Id = entity.Id,
                ClassId = entity.ClassId,
                ClassName = targetClass?.Name ?? string.Empty,
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
        var targetClass = await _classRepository.GetByIdAsync(entity.ClassId);

        return new ScheduleResponseDto
        {
            Id = entity.Id,
            ClassId = entity.ClassId,
            ClassName = targetClass?.Name ?? string.Empty,
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
