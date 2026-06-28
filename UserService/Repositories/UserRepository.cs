using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Entities;

namespace UserService.Repositories;

//UserRepository phải thực hiện các hàm đã khai báo ở IUserRepository
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db; // tạo biến _db để nói chuyện vs postgre

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _db.Users.Include(x => x.TeacherProfile).FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _db.Users.Include(x => x.TeacherProfile)
            .FirstOrDefaultAsync(x => x.Email == usernameOrEmail || x.Username == usernameOrEmail);
    }

    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.Include(x => x.TeacherProfile).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User> UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _db.Users.Include(x => x.TeacherProfile).ToListAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }
}
