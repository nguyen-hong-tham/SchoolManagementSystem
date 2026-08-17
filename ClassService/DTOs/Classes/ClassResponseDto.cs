namespace ClassService.DTOs.Classes;

public class ClassResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int GradeLevel { get; set; }

    public string SchoolYear { get; set; } = string.Empty;

    public int Capacity { get; set; } = 45;

    public int CurrentStudentCount { get; set; }

    public Guid? HomeroomTeacherId { get; set; }

    public string? HomeroomTeacherName { get; set; }

    public string? HomeroomTeacherCode { get; set; }

    public DateTime CreatedAt { get; set; }
}