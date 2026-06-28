using System;
using System.ComponentModel.DataAnnotations;
using ScoreService.Entities;

namespace ScoreService.DtOs;

public class CreateScoreRequest
{
    [Required(ErrorMessage = "Mã học sinh là bắt buộc")]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Mã môn học là bắt buộc")]
    public Guid SubjectId { get; set; }

    [Range(0.0, 10.0, ErrorMessage = "Điểm số phải từ 0.0 đến 10.0")]
    public decimal ScoreValue { get; set; }

    [Required(ErrorMessage = "Loại điểm là bắt buộc")]
    public ScoreType Type { get; set; }

    [Range(1, 2, ErrorMessage = "Học kỳ chỉ có thể là 1 hoặc 2")]
    public int Semester { get; set; }

    [Required(ErrorMessage = "Năm học là bắt buộc")]
    [RegularExpression(
        @"^\d{4}-\d{4}$",
        ErrorMessage = "Định dạng năm học phải là YYYY-YYYY (Ví dụ: 2025-2026)"
    )]
    public string SchoolYear { get; set; } = string.Empty;
}
