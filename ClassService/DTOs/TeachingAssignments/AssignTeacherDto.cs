using System;

namespace ClassService.DTOs.TeachingAssignments;

public class AssignTeacherDto
{
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
}
