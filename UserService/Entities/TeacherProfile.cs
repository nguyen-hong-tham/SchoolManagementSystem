using System;

namespace UserService.Entities;

public class TeacherProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User User { get; set; } = null!;

    public string AcademicDegree { get; set; } = string.Empty; // Học vị
    public string Specialization { get; set; } = string.Empty; // Chuyên môn
    public DateTime HireDate { get; set; } // Ngày tuyển dụng
    public string Department { get; set; } = string.Empty; // Khoa/Bộ môn
}
