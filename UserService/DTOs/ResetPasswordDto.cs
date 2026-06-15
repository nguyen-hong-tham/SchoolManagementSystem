using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
