using System.ComponentModel.DataAnnotations;
using ScoreService.Entities;

namespace ScoreService.DtOs;

public class ScoreResponse
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
    public string Type { get; set; } = string.Empty;
    public int Semester { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
