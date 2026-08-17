using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit; //của MassTransit vào constructor để phát sự kiện lên Broker khi cơ sở dữ liệu thay đổi thành công:
using Microsoft.Extensions.Logging;
using Shared.Events;
using UserService.DTOs;
using UserService.Entities;
using UserService.Repositories;

namespace UserService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtService _jwtService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        JwtService jwtService,
        IPublishEndpoint publishEndpoint,
        ILogger<AuthService> logger
    )
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    // phát event khi đăng kí mới
    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new Exception("Email đã tồn tại");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.UserCode, // Tên tài khoản chính là mã số sinh viên/mã giáo viên
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"), // Mật khẩu cấp ban đầu là 12345678
            Role = UserRole.Student, // đang mặc định là student
            UserCode = request.UserCode,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address ?? string.Empty,
            ClassId = null, // StudentClass is single source of truth
            StudentStatus = StudentStatus.Active
        };
        await _userRepository.CreateAsync(user);

        //Publish event
        var userCreatedEvent = new UserCreatedEvent
        {
            Id = user.Id,
            UserCode = user.UserCode,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            ClassId = user.ClassId,
            StudentStatus = user.StudentStatus?.ToString(),
        };
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(userCreatedEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Publish Info] Failed to publish UserCreatedEvent in background: {ex.Message}");
            }
        });
        return user;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);
        if (user == null)
        {
            throw new Exception("Tài khoản hoặc email không tồn tại");
        }
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            throw new Exception("Mật khẩu không đúng");
        }
        var token = _jwtService.GenerateToken(user);
        return new LoginResponse { Token = token, User = user };
    }

    public async Task UpdateRole(Guid userId, UpdateRoleRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        if (!Enum.TryParse<UserRole>(request.Role, true, out var newRole))
        {
            throw new ArgumentException("Invalid role name");
        }
        user.Role = newRole;
        await _userRepository.UpdateAsync(user);

        // Publish event in background
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(
                    new UserUpdatedEvent
                    {
                        Id = user.Id,
                        UserCode = user.UserCode,
                        FullName = user.FullName,
                        Role = user.Role.ToString(),
                        ClassId = user.ClassId,
                        StudentStatus = user.StudentStatus?.ToString(),
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Publish Info] Failed to publish UserUpdatedEvent in background: {ex.Message}");
            }
        });
    }

    public async Task<List<User>> GetUsers()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<User?> GetUserById(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        return user;
    }

    public async Task<User> UpdateUser(Guid id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Check duplicate email if it's changing
        if (user.Email != request.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email đã tồn tại");
            }
        }

        user.FullName = request.FullName;
        user.Email = request.Email;
        // user.UserCode = request.UserCode; // Không cho phép thay đổi mã học sinh / giáo viên
        user.Gender = request.Gender;
        user.DateOfBirth = request.DateOfBirth;
        user.PhoneNumber = request.PhoneNumber;
        user.Address = request.Address ?? string.Empty;
        // user.ClassId = request.ClassId; // ClassId write removed from UserService to prevent out-of-sync writes

        if (user.Role == UserRole.Teacher)
        {
            if (user.TeacherProfile == null)
            {
                user.TeacherProfile = new TeacherProfile { Id = Guid.NewGuid(), UserId = user.Id };
            }
            user.TeacherProfile.AcademicDegree = request.AcademicDegree ?? string.Empty;
            user.TeacherProfile.Specialization = request.Specialization ?? string.Empty;
            user.TeacherProfile.HireDate = request.HireDate ?? DateTime.UtcNow;
            user.TeacherProfile.Department = request.Department ?? string.Empty;
        }

        var updatedUser = await _userRepository.UpdateAsync(user);

        // Publish event in background
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(
                    new UserUpdatedEvent
                    {
                        Id = updatedUser.Id,
                        UserCode = updatedUser.UserCode,
                        FullName = updatedUser.FullName,
                        Role = updatedUser.Role.ToString(),
                        ClassId = updatedUser.ClassId,
                        StudentStatus = updatedUser.StudentStatus?.ToString(),
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Publish Info] Failed to publish UserUpdatedEvent in background: {ex.Message}");
            }
        });

        return updatedUser;
    }

    public async Task DeleteUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        await _userRepository.DeleteAsync(user);

        // Publish event in background
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(new UserDeletedEvent { Id = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Publish Info] Failed to publish UserDeletedEvent in background: {ex.Message}");
            }
        });
    }

    public async Task<User?> GetStudent(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Student)
        {
            throw new KeyNotFoundException("Student not found");
        }
        return user;
    }

    public async Task<User?> GetTeacher(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Teacher)
        {
            throw new KeyNotFoundException("Teacher not found");
        }
        return user;
    }

    public async Task<List<User>> GetStudents()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return users.Where(u => u.Role == UserRole.Student).ToList();
    }

    public async Task<List<User>> GetTeachers()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return users.Where(u => u.Role == UserRole.Teacher).ToList();
    }

    public async Task DeleteStudent(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Student)
        {
            throw new KeyNotFoundException("Student not found");
        }
        await _userRepository.DeleteAsync(user);

        // Publish event in background
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(new UserDeletedEvent { Id = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Publish Info] Failed to publish UserDeletedEvent in background: {ex.Message}");
            }
        });
    }

    public async Task DeleteTeacher(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Teacher)
        {
            throw new KeyNotFoundException("Teacher not found");
        }
        await _userRepository.DeleteAsync(user);

        // Publish event in background
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(new UserDeletedEvent { Id = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Publish Info] Failed to publish UserDeletedEvent in background: {ex.Message}");
            }
        });
    }

    // phát sự kiện khi admin tạo user mới
    public async Task<User> CreateUserAsync(AdminCreateUserRequest request)
    {
        var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            throw new InvalidOperationException("Email đã tồn tại.");
        }

        var existingUserCode = await _userRepository.GetByUserCodeAsync(request.UserCode);
        if (existingUserCode != null)
        {
            throw new InvalidOperationException($"Mã số '{request.UserCode}' đã tồn tại.");
        }

        string username = !string.IsNullOrWhiteSpace(request.Username) ? request.Username.Trim() : request.UserCode.Trim();
        var existingUsername = await _userRepository.GetByUsernameAsync(username);
        if (existingUsername != null)
        {
            throw new InvalidOperationException($"Tên đăng nhập '{username}' đã tồn tại.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
        {
            throw new ArgumentException("Invalid role. Role must be Admin, Teacher, or Student.");
        }

        // Age validation
        int age = DateTime.Today.Year - request.DateOfBirth.Year;
        if (request.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

        if (parsedRole == UserRole.Teacher)
        {
            if (age < 21)
            {
                throw new InvalidOperationException("Giáo viên phải từ 21 tuổi trở lên (ngày sinh không hợp lệ).");
            }
            if (request.HireDate.HasValue)
            {
                if (request.HireDate.Value.Date > DateTime.Today && request.HireDate.Value.Date > DateTime.UtcNow.Date)
                {
                    throw new InvalidOperationException("Ngày ký hợp đồng không được ở tương lai.");
                }
                if (request.HireDate.Value.Date < request.DateOfBirth.Date.AddYears(18))
                {
                    throw new InvalidOperationException("Ngày ký hợp đồng phải sau ngày sinh ít nhất 18 năm.");
                }
            }
        }
        else if (parsedRole == UserRole.Student)
        {
            if (age < 14 || age > 20)
            {
                throw new InvalidOperationException("Độ tuổi của học sinh cấp 3 (khối 10, 11, 12) phải từ 14 đến 20 tuổi.");
            }
        }
        else if (parsedRole == UserRole.Admin)
        {
            if (age < 18)
            {
                throw new InvalidOperationException("Quản trị viên phải từ 18 tuổi trở lên.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneTaken = await _userRepository.IsPhoneTakenAsync(request.PhoneNumber.Trim());
            if (phoneTaken)
            {
                throw new InvalidOperationException($"Số điện thoại '{request.PhoneNumber}' đã được sử dụng.");
            }
        }

        string password = !string.IsNullOrWhiteSpace(request.Password) ? request.Password : "12345678";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = parsedRole,
            UserCode = request.UserCode.Trim(),
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            Address = request.Address ?? string.Empty,
            ClassId = null, // StudentClass is single source of truth
            StudentStatus = parsedRole == UserRole.Student ? StudentStatus.Active : null
        };

        if (parsedRole == UserRole.Teacher)
        {
            user.TeacherProfile = new TeacherProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                AcademicDegree = request.AcademicDegree ?? string.Empty,
                Specialization = request.Specialization ?? string.Empty,
                HireDate = request.HireDate ?? DateTime.UtcNow,
                Department = request.Department ?? string.Empty,
            };
        }

        var createdUser = await _userRepository.CreateAsync(user);

        // Publish event in background
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(
                    new UserCreatedEvent
                    {
                        Id = createdUser.Id,
                        UserCode = createdUser.UserCode,
                        FullName = createdUser.FullName,
                        Role = createdUser.Role.ToString(),
                        ClassId = createdUser.ClassId,
                        StudentStatus = createdUser.StudentStatus?.ToString(),
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish UserCreatedEvent for user {UserId}", createdUser.Id);
            }
        });

        return createdUser;
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        bool isValidPassword = BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash);
        if (!isValidPassword)
        {
            throw new Exception("Mật khẩu cũ không chính xác");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);
    }

    public async Task ResetPasswordAsync(Guid targetUserId, ResetPasswordDto dto, string actorRole)
    {
        var targetUser = await _userRepository.GetByIdAsync(targetUserId);
        if (targetUser == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Kiểm tra phân quyền:
        // - Admin: Được phép reset mật khẩu của bất kỳ ai.
        // - Teacher: Chỉ được phép reset mật khẩu của học sinh (Role == Student).
        if (actorRole == "Teacher" && targetUser.Role != UserRole.Student)
        {
            throw new UnauthorizedAccessException(
                "Giảng viên chỉ được phép đổi mật khẩu của Sinh viên."
            );
        }

        if (actorRole != "Admin" && actorRole != "Teacher")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        targetUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(targetUser);
    }

    public async Task SyncClassAsync(Guid studentId, Guid? newClassId, string? status)
    {
        var user = await _userRepository.GetByIdAsync(studentId);
        if (user != null)
        {
            user.ClassId = newClassId;
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<StudentStatus>(status, out var parsedStatus))
                {
                    user.StudentStatus = parsedStatus;
                }
            }
            await _userRepository.UpdateAsync(user);
        }
    }

    public async Task<object> CheckAvailabilityAsync(string? userCode, string? username, string? email, string? phoneNumber, Guid? excludeId)
    {
        bool isUserCodeTaken = !string.IsNullOrWhiteSpace(userCode) && await _userRepository.IsUserCodeTakenAsync(userCode, excludeId);
        bool isUsernameTaken = !string.IsNullOrWhiteSpace(username) && await _userRepository.IsUsernameTakenAsync(username, excludeId);
        bool isEmailTaken = !string.IsNullOrWhiteSpace(email) && await _userRepository.IsEmailTakenAsync(email, excludeId);
        bool isPhoneTaken = !string.IsNullOrWhiteSpace(phoneNumber) && await _userRepository.IsPhoneTakenAsync(phoneNumber, excludeId);

        return new
        {
            isUserCodeTaken,
            isUsernameTaken,
            isEmailTaken,
            isPhoneTaken
        };
    }

    public async Task<string> GetNextUserCodeAsync(string role)
    {
        return await _userRepository.GetNextUserCodeAsync(role);
    }
}
