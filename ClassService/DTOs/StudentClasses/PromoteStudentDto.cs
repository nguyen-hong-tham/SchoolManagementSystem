namespace ClassService.DTOs.StudentClasses;

public class PromoteStudentDto
{
    public Guid NewClassId { get; set; }

    public string SchoolYear { get; set; } = string.Empty;
}