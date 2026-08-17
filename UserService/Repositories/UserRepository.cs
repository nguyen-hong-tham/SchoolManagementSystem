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

    public async Task<User?> GetByUserCodeAsync(string userCode)
    {
        return await _db.Users.Include(x => x.TeacherProfile)
            .FirstOrDefaultAsync(x => x.UserCode == userCode);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _db.Users.Include(x => x.TeacherProfile)
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var query = _db.Users.AsQueryable();
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
        return await query.AnyAsync(u => u.Email.ToLower() == email.ToLower().Trim());
    }

    public async Task<bool> IsUserCodeTakenAsync(string userCode, Guid? excludeUserId = null)
    {
        if (string.IsNullOrWhiteSpace(userCode)) return false;
        var query = _db.Users.AsQueryable();
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
        return await query.AnyAsync(u => u.UserCode.ToLower() == userCode.ToLower().Trim());
    }

    public async Task<bool> IsUsernameTakenAsync(string username, Guid? excludeUserId = null)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        var query = _db.Users.AsQueryable();
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
        return await query.AnyAsync(u => u.Username.ToLower() == username.ToLower().Trim());
    }

    public async Task<bool> IsPhoneTakenAsync(string phone, Guid? excludeUserId = null)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        var query = _db.Users.AsQueryable();
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
        return await query.AnyAsync(u => u.PhoneNumber == phone.Trim());
    }

    public async Task<string> GetNextUserCodeAsync(string role)
    {
        string prefix = "STU";
        if (role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
            prefix = "TEA";
        else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            prefix = "ADM";

        var codes = await _db.Users
            .Where(u => u.UserCode.StartsWith(prefix))
            .Select(u => u.UserCode)
            .ToListAsync();

        int maxNum = 0;
        foreach (var c in codes)
        {
            var numPart = c.Substring(prefix.Length);
            if (int.TryParse(numPart, out int num) && num > maxNum)
            {
                maxNum = num;
            }
        }
        return $"{prefix}{maxNum + 1:D3}";
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
