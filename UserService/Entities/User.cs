namespace UserService.Entities;

public enum UserRole
{
    Admin,
    Student,
    Teacher,
}

public enum Gender
{
    Male,
    Female,
}

public class User
{
    public Guid Id { get; set; }

    public string UserCode { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    // Chỉ Student mới có
    public Guid? ClassId { get; set; }

    // Chỉ Teacher mới có
    public virtual TeacherProfile? TeacherProfile { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
