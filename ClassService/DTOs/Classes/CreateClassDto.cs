namespace ClassService.DTOs.Classes;

public class CreateClassDto
{
    public string Name { get; set; } = string.Empty;

    public int GradeLevel { get; set; }

    public string SchoolYear { get; set; } = string.Empty;
}