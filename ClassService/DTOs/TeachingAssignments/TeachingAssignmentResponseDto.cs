using System;

namespace ClassService.DTOs.TeachingAssignments;

public class TeachingAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid ClassId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}
