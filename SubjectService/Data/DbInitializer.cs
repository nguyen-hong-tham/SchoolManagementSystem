using Microsoft.EntityFrameworkCore;
using SubjectService.Entities;

namespace SubjectService.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(SubjectDbContext context)
    {
        // Tự động chạy migration
        await context.Database.MigrateAsync();

        // Nếu đã có dữ liệu thì không seed nữa
        if (await context.Subjects.AnyAsync())
            return;

        var subjects = new List<Subject>
        {
            // ==========================
            // KHỐI 10
            // ==========================
            new()
            {
                Id = Guid.NewGuid(),
                Code = "MATH10",
                Name = "Toán Học 10",
                Description = "Chương trình Toán lớp 10",
                GradeLevel = 10,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "LIT10",
                Name = "Ngữ Văn 10",
                Description = "Chương trình Ngữ văn lớp 10",
                GradeLevel = 10,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "ENG10",
                Name = "Tiếng Anh 10",
                Description = "Chương trình Tiếng Anh lớp 10",
                GradeLevel = 10,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "PHYS10",
                Name = "Vật Lý 10",
                Description = "Chương trình Vật lý lớp 10",
                GradeLevel = 10,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "CHEM10",
                Name = "Hóa Học 10",
                Description = "Chương trình Hóa học lớp 10",
                GradeLevel = 10,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "BIO10",
                Name = "Sinh Học 10",
                Description = "Chương trình Sinh học lớp 10",
                GradeLevel = 10,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "HIS10",
                Name = "Lịch Sử 10",
                Description = "Chương trình Lịch sử lớp 10",
                GradeLevel = 10,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "GEO10",
                Name = "Địa Lý 10",
                Description = "Chương trình Địa lý lớp 10",
                GradeLevel = 10,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "IT10",
                Name = "Tin Học 10",
                Description = "Chương trình Tin học lớp 10",
                GradeLevel = 10,
            },
            // ==========================
            // KHỐI 11
            // ==========================
            new()
            {
                Id = Guid.NewGuid(),
                Code = "MATH11",
                Name = "Toán Học 11",
                Description = "Chương trình Toán lớp 11",
                GradeLevel = 11,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "LIT11",
                Name = "Ngữ Văn 11",
                Description = "Chương trình Ngữ văn lớp 11",
                GradeLevel = 11,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "ENG11",
                Name = "Tiếng Anh 11",
                Description = "Chương trình Tiếng Anh lớp 11",
                GradeLevel = 11,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "PHYS11",
                Name = "Vật Lý 11",
                Description = "Chương trình Vật lý lớp 11",
                GradeLevel = 11,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "CHEM11",
                Name = "Hóa Học 11",
                Description = "Chương trình Hóa học lớp 11",
                GradeLevel = 11,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "BIO11",
                Name = "Sinh Học 11",
                Description = "Chương trình Sinh học lớp 11",
                GradeLevel = 11,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "HIS11",
                Name = "Lịch Sử 11",
                Description = "Chương trình Lịch sử lớp 11",
                GradeLevel = 11,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "GEO11",
                Name = "Địa Lý 11",
                Description = "Chương trình Địa lý lớp 11",
                GradeLevel = 11,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "IT11",
                Name = "Tin Học 11",
                Description = "Chương trình Tin học lớp 11",
                GradeLevel = 11,
            },
            // ==========================
            // KHỐI 12
            // ==========================
            new()
            {
                Id = Guid.NewGuid(),
                Code = "MATH12",
                Name = "Toán Học 12",
                Description = "Chương trình Toán lớp 12",
                GradeLevel = 12,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "LIT12",
                Name = "Ngữ Văn 12",
                Description = "Chương trình Ngữ văn lớp 12",
                GradeLevel = 12,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "ENG12",
                Name = "Tiếng Anh 12",
                Description = "Chương trình Tiếng Anh lớp 12",
                GradeLevel = 12,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "PHYS12",
                Name = "Vật Lý 12",
                Description = "Chương trình Vật lý lớp 12",
                GradeLevel = 12,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "CHEM12",
                Name = "Hóa Học 12",
                Description = "Chương trình Hóa học lớp 12",
                GradeLevel = 12,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "BIO12",
                Name = "Sinh Học 12",
                Description = "Chương trình Sinh học lớp 12",
                GradeLevel = 12,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "HIS12",
                Name = "Lịch Sử 12",
                Description = "Chương trình Lịch sử lớp 12",
                GradeLevel = 12,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "GEO12",
                Name = "Địa Lý 12",
                Description = "Chương trình Địa lý lớp 12",
                GradeLevel = 12,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Code = "IT12",
                Name = "Tin Học 12",
                Description = "Chương trình Tin học lớp 12",
                GradeLevel = 12,
            },
        };

        await context.Subjects.AddRangeAsync(subjects);
        await context.SaveChangesAsync();
    }
}
