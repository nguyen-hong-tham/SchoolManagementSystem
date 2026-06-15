using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassService.DTOs.Schedules;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using ClassService.Services.Interfaces;

namespace ClassService.Services;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IClassRepository _classRepository;

    public ScheduleService(
        IScheduleRepository scheduleRepository,
        IClassRepository classRepository)
    {
        _scheduleRepository = scheduleRepository;
        _classRepository = classRepository;
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

        return MapToResponseDto(entity);
    }

    public async Task<IEnumerable<ScheduleResponseDto>> GetClassScheduleAsync(Guid classId, string schoolYear)
    {
        var targetClass = await _classRepository.GetByIdAsync(classId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {classId}");
        }

        var schedules = await _scheduleRepository.GetScheduleByClassAsync(classId, schoolYear);
        return schedules.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<ScheduleResponseDto>> GetTeacherScheduleAsync(Guid teacherId, string schoolYear)
    {
        var schedules = await _scheduleRepository.GetScheduleByTeacherAsync(teacherId, schoolYear);
        return schedules.Select(MapToResponseDto);
    }

    private static ScheduleResponseDto MapToResponseDto(Schedule entity)
    {
        return new ScheduleResponseDto
        {
            Id = entity.Id,
            ClassId = entity.ClassId,
            SubjectId = entity.SubjectId,
            TeacherId = entity.TeacherId,
            DayOfWeek = entity.DayOfWeek,
            Period = entity.Period,
            Room = entity.Room,
            SchoolYear = entity.SchoolYear
        };
    }
}
