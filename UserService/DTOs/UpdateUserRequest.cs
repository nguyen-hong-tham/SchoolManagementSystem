using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Full name must be between 3 and 100 characters"
        )]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "User code is required")]
        public string UserCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        public UserService.Entities.Gender Gender { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public Guid? ClassId { get; set; }

        // Teacher profile specific fields
        public string? AcademicDegree { get; set; }
        public string? Specialization { get; set; }
        public DateTime? HireDate { get; set; }
        public string? Department { get; set; }
    }
}
