using System;
using System.Collections.Generic;
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

        // Xóa sạch dữ liệu cũ để tránh trùng lặp và áp dụng seed mới
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();

        var users = new List<User>();

        // 1. Admin (1)
        users.Add(
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
            }
        );

        // 2. Teachers (4)
        users.Add(
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
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
                    Department = "Khoa Tự nhiên",
                },
            }
        );

        users.Add(
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
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
                    Department = "Khoa Xã hội",
                },
            }
        );

        users.Add(
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                UserCode = "TEA003",
                Username = "teacher.chinh",
                Email = "chinh.pham@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                FullName = "Phạm Minh Chính",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1980, 11, 25, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000023",
                Address = "Đà Nẵng, Việt Nam",
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow,
                TeacherProfile = new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    AcademicDegree = "Thạc sĩ",
                    Specialization = "Tiếng Anh",
                    HireDate = new DateTime(2010, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    Department = "Khoa Ngoại ngữ",
                },
            }
        );

        users.Add(
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                UserCode = "TEA004",
                Username = "teacher.dung",
                Email = "dung.hoang@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                FullName = "Hoàng Văn Dũng",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1982, 3, 12, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000024",
                Address = "Cần Thơ, Việt Nam",
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow,
                TeacherProfile = new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    AcademicDegree = "Cử nhân",
                    Specialization = "Vật lý",
                    HireDate = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    Department = "Khoa Tự nhiên",
                },
            }
        );

        users.Add(
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                UserCode = "TEA005",
                Username = "teacher.chi",
                Email = "chi.nguyen@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                FullName = "Nguyễn Thị Kim Chi",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1989, 4, 18, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000025",
                Address = "Hà Nội, Việt Nam",
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow,
                TeacherProfile = new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    AcademicDegree = "Thạc sĩ",
                    Specialization = "Hóa học",
                    HireDate = new DateTime(2016, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    Department = "Khoa Tự nhiên",
                },
            }
        );

        users.Add(
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000007"),
                UserCode = "TEA006",
                Username = "teacher.giang",
                Email = "giang.pham@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                FullName = "Phạm Văn Giang",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1991, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000026",
                Address = "Hà Nội, Việt Nam",
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow,
                TeacherProfile = new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    AcademicDegree = "Cử nhân",
                    Specialization = "Sinh học",
                    HireDate = new DateTime(2019, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    Department = "Khoa Tự nhiên",
                },
            }
        );

        users.Add(
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000008"),
                UserCode = "TEA007",
                Username = "teacher.hanh",
                Email = "hanh.do@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                FullName = "Đỗ Thị Hạnh",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1993, 2, 14, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000027",
                Address = "Đà Nẵng, Việt Nam",
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow,
                TeacherProfile = new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    AcademicDegree = "Cử nhân",
                    Specialization = "Lịch sử",
                    HireDate = new DateTime(2020, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    Department = "Khoa Xã hội",
                },
            }
        );

        users.Add(
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000009"),
                UserCode = "TEA008",
                Username = "teacher.hai",
                Email = "hai.nguyen@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                FullName = "Nguyễn Văn Hải",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1984, 7, 25, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000028",
                Address = "Hồ Chí Minh, Việt Nam",
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow,
                TeacherProfile = new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    AcademicDegree = "Thạc sĩ",
                    Specialization = "Địa lý",
                    HireDate = new DateTime(2014, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    Department = "Khoa Xã hội",
                },
            }
        );

        users.Add(
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                UserCode = "TEA009",
                Username = "teacher.tuan",
                Email = "tuan.le@school.edu.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                FullName = "Lê Minh Tuấn",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1990, 12, 5, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "0900000029",
                Address = "Bình Dương, Việt Nam",
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow,
                TeacherProfile = new TeacherProfile
                {
                    Id = Guid.NewGuid(),
                    AcademicDegree = "Kỹ sư",
                    Specialization = "Tin học",
                    HireDate = new DateTime(2017, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    Department = "Khoa Tự nhiên",
                },
            }
        );


        // 3. Students (20)
        string[] studentNames =
        {
            "Trần Quốc Cường",
            "Phạm Thị Dũng",
            "Nguyễn Hoàng Nam",
            "Lê Hoài Nam",
            "Phan Thanh Bình",
            "Nguyễn Thị Mai",
            "Trần Văn Hùng",
            "Lê Thị Hương",
            "Phạm Văn Khoa",
            "Đỗ Thị Lan",
            "Ngô Văn Minh",
            "Bùi Thị Nga",
            "Dương Văn Oanh",
            "Vũ Thị Phương",
            "Đặng Văn Quỳnh",
            "Hoàng Thị Thảo",
            "Bùi Văn Tâm",
            "Nguyễn Minh Triết",
            "Phạm Hồng Sơn",
            "Lê Minh Thư"
        };

        string[] studentUsernames =
        {
            "student.cuong",
            "student.dung",
            "student.nam1",
            "student.nam2",
            "student.binh",
            "student.mai",
            "student.hung",
            "student.huong",
            "student.khoa",
            "student.lan",
            "student.minh",
            "student.nga",
            "student.oanh",
            "student.phuong",
            "student.quynh",
            "student.thao",
            "student.tam",
            "student.triet",
            "student.son",
            "student.thu"
        };

        Guid[] studentClasses =
        {
            Guid.Parse("00000000-0000-0000-0000-000000010101"), // 10A1
            Guid.Parse("00000000-0000-0000-0000-000000010101"),
            Guid.Parse("00000000-0000-0000-0000-000000010101"),
            Guid.Parse("00000000-0000-0000-0000-000000010101"),
            Guid.Parse("00000000-0000-0000-0000-000000010101"),
            Guid.Parse("00000000-0000-0000-0000-000000010102"), // 10A2
            Guid.Parse("00000000-0000-0000-0000-000000010102"),
            Guid.Parse("00000000-0000-0000-0000-000000010102"),
            Guid.Parse("00000000-0000-0000-0000-000000010102"),
            Guid.Parse("00000000-0000-0000-0000-000000010102"),
            Guid.Parse("00000000-0000-0000-0000-000000011101"), // 11A1
            Guid.Parse("00000000-0000-0000-0000-000000011101"),
            Guid.Parse("00000000-0000-0000-0000-000000011101"),
            Guid.Parse("00000000-0000-0000-0000-000000011101"),
            Guid.Parse("00000000-0000-0000-0000-000000011101"),
            Guid.Parse("00000000-0000-0000-0000-000000012101"), // 12A1
            Guid.Parse("00000000-0000-0000-0000-000000012101"),
            Guid.Parse("00000000-0000-0000-0000-000000012101"),
            Guid.Parse("00000000-0000-0000-0000-000000012101"),
            Guid.Parse("00000000-0000-0000-0000-000000012101"),
        };

        for (int i = 0; i < 20; i++)
        {
            int indexNum = i + 1;
            string code = $"STU{indexNum:D3}";
            Guid studentId = Guid.Parse($"00000000-0000-0000-0000-0000000000{indexNum + 10:D2}");

            users.Add(
                new User
                {
                    Id = studentId,
                    UserCode = code,
                    Username = studentUsernames[i],
                    Email = $"{studentUsernames[i]}@school.edu.vn",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                    FullName = studentNames[i],
                    Gender = i % 2 == 0 ? Gender.Male : Gender.Female,
                    DateOfBirth = new DateTime(2010, 1, indexNum, 0, 0, 0, DateTimeKind.Utc),
                    PhoneNumber = $"09000000{indexNum + 10:D2}",
                    Address = "Học sinh tại trường",
                    Role = UserRole.Student,
                    ClassId = studentClasses[i],
                    StudentStatus = StudentStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                }
            );
        }

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
