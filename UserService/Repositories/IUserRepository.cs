using UserService.Entities;

namespace UserService.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email); // tìm user theo email
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail); // tìm user theo username hoặc email
    Task<User> CreateAsync(User user); // tao mới user
    Task<User?> GetByIdAsync(Guid id);
    Task<User> UpdateAsync(User user);
    Task<List<User>> GetAllUsersAsync();
    Task DeleteAsync(User user);
}
