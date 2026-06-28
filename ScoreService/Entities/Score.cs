using System;

namespace ScoreService.Entities;

public enum ScoreType
{
    Oral = 1, // điểm miệng
    FifteenMinutes = 2, // điểm 15 phút
    OnePeriod = 3, // điểm 1 tiết (45 phút)
    MidTerm = 4, // điểm giữa kì
    Final = 5, // điểm cuối kì
}

public class Score
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; } // Khóa ngoại ảo đến bảng CachedUsers
    public Guid SubjectId { get; set; } // Khóa ngoại ảo đến bảng CachedSubjects
    public Guid TeacherId { get; set; } // Khóa ngoại ảo đến bảng CachedUsers (người nhập điểm)

    public decimal ScoreValue { get; set; } // Điểm số (0.0 -> 10.0)
    public ScoreType Type { get; set; } // Loại điểm (Miệng, 15p, 1 tiết, Giữa kỳ, Cuối kỳ)
    public int Semester { get; set; } // Học kỳ (1 hoặc 2)
    public string SchoolYear { get; set; } = string.Empty; // Ví dụ: "2025-2026"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties đệm
    public CachedUser? Student { get; set; }
    public CachedSubject? Subject { get; set; }
}
