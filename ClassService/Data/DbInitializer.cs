using System;
using System.Linq;
using System.Threading.Tasks;
using ClassService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Tự động áp dụng Migrations
        await context.Database.MigrateAsync();

        // Nếu đã có lớp học, không cần seed nữa
        if (context.Classes.Any())
        {
            return;
        }

        // 1. Seed Classes
        var class1 = new Class
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000010101"),
            Name = "10A1",
            GradeLevel = 10,
            SchoolYear = "2025-2026",
            CreatedAt = DateTime.UtcNow,
        };

        var class2 = new Class
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000011101"),
            Name = "11A1",
            GradeLevel = 11,
            SchoolYear = "2025-2026",
            CreatedAt = DateTime.UtcNow,
        };

        await context.Classes.AddRangeAsync(class1, class2);

        // 2. Seed StudentClass
        // Giả sử đây là ID Guid của các học sinh bên hệ thống User (hoặc được sinh ngẫu nhiên)
        var student1Id = Guid.Parse("00000000-0000-0000-0000-000000000004");
        var student2Id = Guid.Parse("00000000-0000-0000-0000-000000000005");

        var sc1 = new StudentClass
        {
            Id = Guid.NewGuid(),
            StudentId = student1Id,
            ClassId = class1.Id,
            SchoolYear = "2025-2026",
            AssignedDate = DateTime.UtcNow,
            IsCurrent = true,
        };

        var sc2 = new StudentClass
        {
            Id = Guid.NewGuid(),
            StudentId = student2Id,
            ClassId = class2.Id,
            SchoolYear = "2025-2026",
            AssignedDate = DateTime.UtcNow,
            IsCurrent = true,
        };

        await context.StudentClasses.AddRangeAsync(sc1, sc2);

        // 3. Seed HomeroomAssignment
        // Giả sử đây là ID Guid của các giáo viên
        var teacher1Id = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var teacher2Id = Guid.Parse("00000000-0000-0000-0000-000000000003");

        var hr1 = new HomeroomAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = teacher1Id,
            ClassId = class1.Id,
            SchoolYear = "2025-2026",
            AssignedDate = DateTime.UtcNow,
        };

        await context.HomeroomAssignments.AddAsync(hr1);

        // 4. Seed TeachingAssignment
        // SubjectId đại diện cho các môn học Toán và Văn
        var mathSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000011");
        var literatureSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000012");

        var ta1 = new TeachingAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = teacher1Id,
            SubjectId = mathSubjectId,
            ClassId = class1.Id,
            SchoolYear = "2025-2026",
            AssignedDate = DateTime.UtcNow,
        };

        var ta2 = new TeachingAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = teacher2Id,
            SubjectId = literatureSubjectId,
            ClassId = class1.Id,
            SchoolYear = "2025-2026",
            AssignedDate = DateTime.UtcNow,
        };

        await context.TeachingAssignments.AddRangeAsync(ta1, ta2);

        // 5. Seed Schedule (Thời khóa biểu)
        var schedule1 = new Schedule
        {
            Id = Guid.NewGuid(),
            ClassId = class1.Id,
            SubjectId = mathSubjectId,
            TeacherId = teacher1Id,
            DayOfWeek = 2, // Thứ Hai
            Period = 1, // Tiết 1
            Room = "Phòng 101",
            SchoolYear = "2025-2026",
        };

        var schedule2 = new Schedule
        {
            Id = Guid.NewGuid(),
            ClassId = class1.Id,
            SubjectId = literatureSubjectId,
            TeacherId = teacher2Id,
            DayOfWeek = 2, // Thứ Hai
            Period = 2, // Tiết 2
            Room = "Phòng 101",
            SchoolYear = "2025-2026",
        };

        await context.Schedules.AddRangeAsync(schedule1, schedule2);

        await context.SaveChangesAsync();
    }
}
