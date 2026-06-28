using System;

namespace ScoreService.Entities;

public class CachedUser
{
    public Guid Id { get; set; } // Trùng khớp ID từ UserService
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Student" hoặc "Teacher"
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
