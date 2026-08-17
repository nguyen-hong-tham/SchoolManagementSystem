using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassService.DTOs.Schedules;

namespace ClassService.Services.Interfaces;

public interface IScheduleService
{
    Task<ScheduleResponseDto> CreateScheduleAsync(CreateScheduleDto dto);
    Task<ScheduleResponseDto> UpdateScheduleAsync(Guid id, UpdateScheduleDto dto);
    Task DeleteScheduleAsync(Guid id);
    Task ClearClassScheduleAsync(Guid classId, string? schoolYear);
    Task<IEnumerable<ScheduleResponseDto>> GetClassScheduleAsync(Guid classId, string? schoolYear);
    Task<IEnumerable<ScheduleResponseDto>> GetTeacherScheduleAsync(Guid teacherId, string? schoolYear);
}
