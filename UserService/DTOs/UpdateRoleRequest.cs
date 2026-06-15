using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs;

public class UpdateRoleRequest
{
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty;
}