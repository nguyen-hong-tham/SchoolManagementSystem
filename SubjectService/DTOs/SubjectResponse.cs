using System.ComponentModel.DataAnnotations;

namespace SubjectService.DTOs;

public class SubjectResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public DateTime CreatedAt { get; set; }
}
