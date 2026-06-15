using System;

namespace ClassService.Entities;

public class CachedSubject
{
    public Guid Id { get; set; } // Trùng khớp ID bên SubjectService
    public string Code { get; set; } = string.Empty; // TOAN10, VAN11, etc.
    public string Name { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
