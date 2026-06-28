namespace ClassService.Entities;

public class Class
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int GradeLevel { get; set; }

    public string SchoolYear { get; set; } = string.Empty;

    public int Capacity { get; set; } = 40;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
}