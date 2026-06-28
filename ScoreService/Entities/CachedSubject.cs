using System;

namespace ScoreService.Entities;

public class CachedSubject
{
    public Guid Id { get; set; } // Trùng khớp ID từ SubjectService
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
