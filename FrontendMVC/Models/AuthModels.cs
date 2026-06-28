using System;
using System.ComponentModel.DataAnnotations;

namespace FrontendMVC.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email hoặc Username là bắt buộc")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class UserViewModel
{
    public Guid Id { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Role { get; set; }
    public int Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid? ClassId { get; set; }
    public DateTime CreatedAt { get; set; }
    public TeacherProfileViewModel? TeacherProfile { get; set; }
}

public class TeacherProfileViewModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AcademicDegree { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public string Department { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserViewModel User { get; set; } = null!;
}
