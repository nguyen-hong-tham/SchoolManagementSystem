namespace ClassService.DTOs.StudentClasses;

public class StudentClassResponseDto
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string StudentCode { get; set; } = string.Empty;

    public Guid ClassId { get; set; }
    
    public string ClassName { get; set; } = string.Empty;

    public string SchoolYear { get; set; } = string.Empty;

    public DateTime AssignedDate { get; set; }

    public bool IsCurrent { get; set; }

    public DateTime? LeftDate { get; set; }

    public string? PromotionStatus { get; set; }
}