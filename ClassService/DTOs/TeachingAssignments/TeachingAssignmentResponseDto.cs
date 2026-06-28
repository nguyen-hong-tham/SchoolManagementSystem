using System;

namespace ClassService.DTOs.TeachingAssignments;

public class TeachingAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherCode { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}
