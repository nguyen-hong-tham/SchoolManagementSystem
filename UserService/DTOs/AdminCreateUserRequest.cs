using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class AdminCreateUserRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Full name must be between 3 and 100 characters"
        )]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(
            50,
            MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 50 characters"
        )]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters"
        )]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = null!;

        [Required(ErrorMessage = "User code is required")]
        public string UserCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        public UserService.Entities.Gender Gender { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        public string? PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; } = string.Empty;

        public Guid? ClassId { get; set; }

        // Teacher profile specific fields
        public string? AcademicDegree { get; set; }
        public string? Specialization { get; set; }
        public DateTime? HireDate { get; set; }
        public string? Department { get; set; }
    }
}
