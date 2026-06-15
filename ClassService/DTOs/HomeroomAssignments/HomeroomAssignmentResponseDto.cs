using System;

namespace ClassService.DTOs.HomeroomAssignments;

public class HomeroomAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public bool IsCurrent { get; set; }
}
