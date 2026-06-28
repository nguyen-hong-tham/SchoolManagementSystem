using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubjectService.Entities;

namespace SubjectService.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(SubjectDbContext context)
    {
        // Tự động chạy migration
        await context.Database.MigrateAsync();

        // Xóa sạch dữ liệu cũ
        context.Subjects.RemoveRange(context.Subjects);
        await context.SaveChangesAsync();

        var subjects = new List<Subject>
        {
            // KHỐI 10
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000010"), Code = "MATH10", Name = "Toán Học 10", Description = "Chương trình Toán lớp 10", GradeLevel = 10 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000011"), Code = "LIT10", Name = "Ngữ Văn 10", Description = "Chương trình Ngữ văn lớp 10", GradeLevel = 10 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000012"), Code = "ENG10", Name = "Tiếng Anh 10", Description = "Chương trình Tiếng Anh lớp 10", GradeLevel = 10 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000013"), Code = "PHYS10", Name = "Vật Lý 10", Description = "Chương trình Vật lý lớp 10", GradeLevel = 10 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000014"), Code = "CHEM10", Name = "Hóa Học 10", Description = "Chương trình Hóa học lớp 10", GradeLevel = 10 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000015"), Code = "BIO10", Name = "Sinh Học 10", Description = "Chương trình Sinh học lớp 10", GradeLevel = 10 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000016"), Code = "HIS10", Name = "Lịch Sử 10", Description = "Chương trình Lịch sử lớp 10", GradeLevel = 10 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000017"), Code = "GEO10", Name = "Địa Lý 10", Description = "Chương trình Địa lý lớp 10", GradeLevel = 10 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000018"), Code = "IT10", Name = "Tin Học 10", Description = "Chương trình Tin học lớp 10", GradeLevel = 10 },

            // KHỐI 11
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000020"), Code = "MATH11", Name = "Toán Học 11", Description = "Chương trình Toán lớp 11", GradeLevel = 11 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000021"), Code = "LIT11", Name = "Ngữ Văn 11", Description = "Chương trình Ngữ văn lớp 11", GradeLevel = 11 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000022"), Code = "ENG11", Name = "Tiếng Anh 11", Description = "Chương trình Tiếng Anh lớp 11", GradeLevel = 11 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000023"), Code = "PHYS11", Name = "Vật Lý 11", Description = "Chương trình Vật lý lớp 11", GradeLevel = 11 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000024"), Code = "CHEM11", Name = "Hóa Học 11", Description = "Chương trình Hóa học lớp 11", GradeLevel = 11 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000025"), Code = "BIO11", Name = "Sinh Học 11", Description = "Chương trình Sinh học lớp 11", GradeLevel = 11 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000026"), Code = "HIS11", Name = "Lịch Sử 11", Description = "Chương trình Lịch sử lớp 11", GradeLevel = 11 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000027"), Code = "GEO11", Name = "Địa Lý 11", Description = "Chương trình Địa lý lớp 11", GradeLevel = 11 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000028"), Code = "IT11", Name = "Tin Học 11", Description = "Chương trình Tin học lớp 11", GradeLevel = 11 },

            // KHỐI 12
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000030"), Code = "MATH12", Name = "Toán Học 12", Description = "Chương trình Toán lớp 12", GradeLevel = 12 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000031"), Code = "LIT12", Name = "Ngữ Văn 12", Description = "Chương trình Ngữ văn lớp 12", GradeLevel = 12 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000032"), Code = "ENG12", Name = "Tiếng Anh 12", Description = "Chương trình Tiếng Anh lớp 12", GradeLevel = 12 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000033"), Code = "PHYS12", Name = "Vật Lý 12", Description = "Chương trình Vật lý lớp 12", GradeLevel = 12 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000034"), Code = "CHEM12", Name = "Hóa Học 12", Description = "Chương trình Hóa học lớp 12", GradeLevel = 12 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000035"), Code = "BIO12", Name = "Sinh Học 12", Description = "Chương trình Sinh học lớp 12", GradeLevel = 12 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000036"), Code = "HIS12", Name = "Lịch Sử 12", Description = "Chương trình Lịch sử lớp 12", GradeLevel = 12 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000037"), Code = "GEO12", Name = "Địa Lý 12", Description = "Chương trình Địa lý lớp 12", GradeLevel = 12 },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000038"), Code = "IT12", Name = "Tin Học 12", Description = "Chương trình Tin học lớp 12", GradeLevel = 12 },
        };

        await context.Subjects.AddRangeAsync(subjects);
        await context.SaveChangesAsync();
    }
}
