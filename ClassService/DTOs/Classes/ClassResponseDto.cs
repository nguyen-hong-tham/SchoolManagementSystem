namespace ClassService.DTOs.Classes;

public class ClassResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int GradeLevel { get; set; }

    public string SchoolYear { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}