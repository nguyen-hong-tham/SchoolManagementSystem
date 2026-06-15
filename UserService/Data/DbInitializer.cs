using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Entities;

namespace UserService.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Tự động áp dụng Migrations nếu chưa được chạy
        await context.Database.MigrateAsync();

        // Nếu đã có người dùng, dừng không seed nữa để tránh trùng lặp
        if (context.Users.Any())
        {
            return;
        }

        var users = new[]
        {
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserCode = "ADM001",
                Username = "admin",
                Email = "admin@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Administrator",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000001",
                Address = "Hệ thống trường học",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), // Trùng khớp teacher1Id bên ClassService
                UserCode = "TEA001",
                Username = "teacher.an",
                Email = "an.nguyen@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                FullName = "Nguyễn Văn An",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1985, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000002",
                Address = "Hà Nội, Việt Nam",
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow,
                TeacherProfile = new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    AcademicDegree = "Thạc sĩ",
                    Specialization = "Toán học",
                    HireDate = new DateTime(2015, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    Department = "Khoa Tự nhiên"
                }
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), // Trùng khớp teacher2Id bên ClassService
                UserCode = "TEA002",
                Username = "teacher.binh",
                Email = "binh.le@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                FullName = "Lê Thị Bình",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1988, 8, 20, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000003",
                Address = "Hải Phòng, Việt Nam",
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow,
                TeacherProfile = new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    AcademicDegree = "Cử nhân",
                    Specialization = "Ngữ văn",
                    HireDate = new DateTime(2018, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    Department = "Khoa Xã hội"
                }
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), // Trùng khớp student1Id bên ClassService
                UserCode = "STU001",
                Username = "student.cuong",
                Email = "cuong.tran@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                FullName = "Trần Quốc Cường",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(2010, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000004",
                Address = "Đà Nẵng, Việt Nam",
                Role = UserRole.Student,
                ClassId = Guid.Parse("00000000-0000-0000-0000-000000010101"), // Trùng khớp class1.Id (Lớp 10A1)
                CreatedAt = DateTime.UtcNow,
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), // Trùng khớp student2Id bên ClassService
                UserCode = "STU002",
                Username = "student.dung",
                Email = "dung.pham@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                FullName = "Phạm Thị Dũng",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(2010, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000005",
                Address = "TP. Hồ Chí Minh, Việt Nam",
                Role = UserRole.Student,
                ClassId = Guid.Parse("00000000-0000-0000-0000-000000011101"), // Trùng khớp class2.Id (Lớp 11A1)
                CreatedAt = DateTime.UtcNow,
            },
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
