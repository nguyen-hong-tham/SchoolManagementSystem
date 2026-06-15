using System;

namespace SubjectService.Entities;

public class Subject
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty; // Ví dụ: TOAN10, VAN11, ANH12

    public string Name { get; set; } = string.Empty; // Ví dụ: Toán học, Ngữ văn

    public string Description { get; set; } = string.Empty;

    public int GradeLevel { get; set; } // Khối lớp từ 1 đến 12.

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
