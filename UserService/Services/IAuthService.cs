using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.DTOs;
using UserService.Entities;

namespace UserService.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterRequest request); // đăng ký tài khoản mới

    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task UpdateRole(Guid id, UpdateRoleRequest request);

    Task<List<User>> GetUsers();

    Task<User?> GetUserById(Guid id);

    Task<User> UpdateUser(Guid id, UpdateUserRequest request);

    Task DeleteUser(Guid id);

    Task<User?> GetStudent(Guid id);

    Task<User?> GetTeacher(Guid id);

    Task<List<User>> GetStudents();

    Task<List<User>> GetTeachers();

    Task DeleteStudent(Guid id);

    Task DeleteTeacher(Guid id);

    Task<User> CreateUserAsync(AdminCreateUserRequest request);

    Task ChangePasswordAsync(Guid id, ChangePasswordDto dto);

    Task ResetPasswordAsync(Guid targetUserId, ResetPasswordDto dto, string actorRole);
}
