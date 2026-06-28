using System;

namespace FrontendMVC.Models;

public class ScoreResponseViewModel
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    
    public Guid TeacherId { get; set; }
    public decimal ScoreValue { get; set; }
    public string Type { get; set; } = string.Empty; // Oral, FifteenMinutes, OnePeriod, MidTerm, Final
    public int Semester { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateScoreRequestViewModel
{
    public Guid StudentId { get; set; }
    public Guid SubjectId { get; set; }
    public decimal ScoreValue { get; set; }
    public string Type { get; set; } = string.Empty; // Oral, FifteenMinutes, OnePeriod, MidTerm, Final
    public int Semester { get; set; }
    public string SchoolYear { get; set; } = "2025-2026";
}

public class UpdateScoreRequestViewModel
{
    public decimal ScoreValue { get; set; }
}
