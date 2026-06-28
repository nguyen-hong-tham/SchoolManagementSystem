using System;
using System.ComponentModel.DataAnnotations;

namespace FrontendMVC.Models;

public enum Gender
{
    [Display(Name = "Nam")]
    Male,
    [Display(Name = "Nữ")]
    Female
}

public enum UserRole
{
    Admin,
    Student,
    Teacher
}

public class AdminCreateUserRequest
{
    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ và tên phải từ 3 đến 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vai trò là bắt buộc")]
    public string Role { get; set; } = string.Empty; // "Admin", "Teacher", "Student"

    [Required(ErrorMessage = "Mã số (học sinh/giáo viên) là bắt buộc")]
    public string UserCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Giới tính là bắt buộc")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    [DataType(DataType.Date)]
    [PastDate(ErrorMessage = "Ngày sinh không được ở tương lai.")]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-15);

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? PhoneNumber { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;

    // Chỉ dùng cho Học sinh
    public Guid? ClassId { get; set; }

    // Chỉ dùng cho Giáo viên
    public string? AcademicDegree { get; set; }
    public string? Specialization { get; set; }
    
    [DataType(DataType.Date)]
    [PastDate(ErrorMessage = "Ngày nhận công tác không được ở tương lai.")]
    public DateTime? HireDate { get; set; }
    
    public string? Department { get; set; }
}

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ và tên phải từ 3 đến 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã số (học sinh/giáo viên) là bắt buộc")]
    public string UserCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Giới tính là bắt buộc")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    [DataType(DataType.Date)]
    [PastDate(ErrorMessage = "Ngày sinh không được ở tương lai.")]
    public DateTime DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? PhoneNumber { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;

    // Chỉ dùng cho Học sinh
    public Guid? ClassId { get; set; }

    // Chỉ dùng cho Giáo viên
    public string? AcademicDegree { get; set; }
    public string? Specialization { get; set; }
    
    [DataType(DataType.Date)]
    [PastDate(ErrorMessage = "Ngày nhận công tác không được ở tương lai.")]
    public DateTime? HireDate { get; set; }
    
    public string? Department { get; set; }
}
