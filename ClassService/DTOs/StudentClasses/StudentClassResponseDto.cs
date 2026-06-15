namespace ClassService.DTOs.StudentClasses;

public class StudentClassResponseDto
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid ClassId { get; set; }

    public string SchoolYear { get; set; } = string.Empty;

    public DateTime AssignedDate { get; set; }

    public bool IsCurrent { get; set; }
}