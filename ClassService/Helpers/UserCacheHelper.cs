using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClassService.Data;
using ClassService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Helpers;

public static class UserCacheHelper
{
    public static async Task<CachedUser?> GetOrFetchCachedUserAsync(ApplicationDbContext dbContext, Guid userId)
    {
        var user = await dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null) return user;

        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync($"http://localhost:5156/api/users/internal/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<CachedUserDto>();
                if (data != null)
                {
                    user = await dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == userId);
                    if (user != null) return user;

                    user = new CachedUser
                    {
                        Id = data.Id,
                        UserCode = data.UserCode ?? string.Empty,
                        FullName = data.FullName ?? string.Empty,
                        Role = data.Role ?? "Student",
                        StudentStatus = data.StudentStatus ?? "Active",
                        LastUpdated = DateTime.UtcNow
                    };
                    dbContext.CachedUsers.Add(user);
                    await dbContext.SaveChangesAsync();
                    return user;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UserCacheHelper] Fallback fetch error for userId {userId}: {ex.Message}");
        }

        return null;
    }
}

public class CachedUserDto
{
    public Guid Id { get; set; }
    public string? UserCode { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public string? StudentStatus { get; set; }
    public Guid? ClassId { get; set; }
}
