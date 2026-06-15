using System;

namespace ClassService.DTOs.Schedules;

public class ScheduleResponseDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid TeacherId { get; set; }
    public int DayOfWeek { get; set; }
    public int Period { get; set; }
    public string Room { get; set; } = string.Empty;
    public string SchoolYear { get; set; } = string.Empty;
}
