using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit; //của MassTransit vào constructor để phát sự kiện lên Broker khi cơ sở dữ liệu thay đổi thành công:
using Shared.Events;
using UserService.DTOs;
using UserService.Entities;
using UserService.Repositories;

namespace UserService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtService _jwtService;
    private readonly IPublishEndpoint _publishEndpoint; // tiêm interface

    public AuthService(
        IUserRepository userRepository,
        JwtService jwtService,
        IPublishEndpoint publishEndpoint
    )
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _publishEndpoint = publishEndpoint;
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
            ClassId = request.ClassId,
        };
        await _userRepository.CreateAsync(user);

        //Publish event
        var userCreatedEvent = new UserCreatedEvent
        {
            Id = user.Id,
            UserCode = user.UserCode,
            FullName = user.FullName,
            Role = user.Role.ToString(),
        };
        await _publishEndpoint.Publish(userCreatedEvent);
        return user;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new Exception("Email không tồn tại");
        }
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            throw new Exception("Mật khẩu không đúng");
        }
        var token = _jwtService.GenerateToken(user);
        return new LoginResponse { Token = token };
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

        // Publish event
        await _publishEndpoint.Publish(
            new UserUpdatedEvent
            {
                Id = user.Id,
                UserCode = user.UserCode,
                FullName = user.FullName,
                Role = user.Role.ToString(),
            }
        );
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
        user.ClassId = request.ClassId;

        if (user.Role == UserRole.Teacher)
        {
            if (user.TeacherProfile == null)
            {
                user.TeacherProfile = new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id
                };
            }
            user.TeacherProfile.AcademicDegree = request.AcademicDegree ?? string.Empty;
            user.TeacherProfile.Specialization = request.Specialization ?? string.Empty;
            user.TeacherProfile.HireDate = request.HireDate ?? DateTime.UtcNow;
            user.TeacherProfile.Department = request.Department ?? string.Empty;
        }

        var updatedUser = await _userRepository.UpdateAsync(user);

        // Publish event
        await _publishEndpoint.Publish(
            new UserUpdatedEvent
            {
                Id = updatedUser.Id,
                UserCode = updatedUser.UserCode,
                FullName = updatedUser.FullName,
                Role = updatedUser.Role.ToString(),
            }
        );

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

        // Publish event
        await _publishEndpoint.Publish(new UserDeletedEvent { Id = id });
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

        // Publish event
        await _publishEndpoint.Publish(new UserDeletedEvent { Id = id });
    }

    public async Task DeleteTeacher(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Teacher)
        {
            throw new KeyNotFoundException("Teacher not found");
        }
        await _userRepository.DeleteAsync(user);

        // Publish event
        await _publishEndpoint.Publish(new UserDeletedEvent { Id = id });
    }

    // phát sự kiện khi admin tạo user mới
    public async Task<User> CreateUserAsync(AdminCreateUserRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email đã tồn tại");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
        {
            throw new ArgumentException("Invalid role. Role must be Admin, Teacher, or Student.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.UserCode, // Tên tài khoản chính là mã giáo viên/học sinh
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"), // Mật khẩu cấp ban đầu là 12345678
            Role = parsedRole,
            UserCode = request.UserCode,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            Address = request.Address ?? string.Empty,
            ClassId = request.ClassId,
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

        // Publish event
        await _publishEndpoint.Publish(
            new UserCreatedEvent
            {
                Id = createdUser.Id,
                UserCode = createdUser.UserCode,
                FullName = createdUser.FullName,
                Role = createdUser.Role.ToString(),
            }
        );

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
            throw new UnauthorizedAccessException("Giáo viên chỉ được phép đổi mật khẩu của Học sinh.");
        }

        if (actorRole != "Admin" && actorRole != "Teacher")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
        }

        targetUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(targetUser);
    }
}
