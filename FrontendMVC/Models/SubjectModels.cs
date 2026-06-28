using System;
using System.ComponentModel.DataAnnotations;

namespace FrontendMVC.Models;

public class SubjectViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Mã môn học là bắt buộc")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Mã môn học chỉ được chứa chữ cái và số")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên môn học là bắt buộc")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Khối lớp là bắt buộc")]
    [Range(1, 12, ErrorMessage = "Khối lớp phải từ 1 đến 12")]
    public int GradeLevel { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateSubjectRequest
{
    [Required(ErrorMessage = "Mã môn học là bắt buộc")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên môn học là bắt buộc")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Khối lớp là bắt buộc")]
    [Range(1, 12, ErrorMessage = "Khối lớp phải từ 1 đến 12")]
    public int GradeLevel { get; set; }
}

public class UpdateSubjectRequest
{
    [Required(ErrorMessage = "Tên môn học là bắt buộc")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Khối lớp là bắt buộc")]
    [Range(1, 12, ErrorMessage = "Khối lớp phải từ 1 đến 12")]
    public int GradeLevel { get; set; }
}
