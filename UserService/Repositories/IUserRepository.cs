using UserService.Entities;

namespace UserService.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email); // tìm user theo email
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail); // tìm user theo username hoặc email
    Task<User?> GetByUserCodeAsync(string userCode); // tìm user theo user code
    Task<User?> GetByUsernameAsync(string username); // tìm user theo username
    Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null);
    Task<bool> IsUserCodeTakenAsync(string userCode, Guid? excludeUserId = null);
    Task<bool> IsUsernameTakenAsync(string username, Guid? excludeUserId = null);
    Task<bool> IsPhoneTakenAsync(string phone, Guid? excludeUserId = null);
    Task<string> GetNextUserCodeAsync(string role);
    Task<User> CreateAsync(User user); // tao mới user
    Task<User?> GetByIdAsync(Guid id);
    Task<User> UpdateAsync(User user);
    Task<List<User>> GetAllUsersAsync();
    Task DeleteAsync(User user);
}
