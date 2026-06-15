using System.ComponentModel.DataAnnotations;

namespace SubjectService.DTOs;

public class UpdateSubjectRequest
{
    [Required(ErrorMessage = "Tên môn học là bắt buộc")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên môn học phải từ 2 đến 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(1, 12, ErrorMessage = "Khối lớp phải nằm trong khoảng từ 1 đến 12")]
    public int GradeLevel { get; set; }
}
