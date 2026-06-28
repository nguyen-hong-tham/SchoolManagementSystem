using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ScoreService.Entities;

namespace ScoreService.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ScoreDbContext context)
    {
        // Tự động chạy migration
        await context.Database.MigrateAsync();

        // Xóa sạch dữ liệu cũ
        context.Scores.RemoveRange(context.Scores);
        context.CachedUsers.RemoveRange(context.CachedUsers);
        context.CachedSubjects.RemoveRange(context.CachedSubjects);
        await context.SaveChangesAsync();

        // 1. Seed CachedUsers (1 Admin, 4 Teachers, 20 Students)
        var cachedUsers = new List<CachedUser>
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserCode = "ADM001",
                FullName = "System Administrator",
                Role = "Admin",
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                UserCode = "TEA001",
                FullName = "Nguyễn Văn An",
                Role = "Teacher",
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                UserCode = "TEA002",
                FullName = "Lê Thị Bình",
                Role = "Teacher",
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                UserCode = "TEA003",
                FullName = "Phạm Minh Chính",
                Role = "Teacher",
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                UserCode = "TEA004",
                FullName = "Hoàng Văn Dũng",
                Role = "Teacher",
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                UserCode = "TEA005",
                FullName = "Nguyễn Thị Kim Chi",
                Role = "Teacher",
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000007"),
                UserCode = "TEA006",
                FullName = "Phạm Văn Giang",
                Role = "Teacher",
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000008"),
                UserCode = "TEA007",
                FullName = "Đỗ Thị Hạnh",
                Role = "Teacher",
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000009"),
                UserCode = "TEA008",
                FullName = "Nguyễn Văn Hải",
                Role = "Teacher",
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                UserCode = "TEA009",
                FullName = "Lê Minh Tuấn",
                Role = "Teacher",
                LastUpdated = DateTime.UtcNow,
            },
        };

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

        for (int i = 0; i < 20; i++)
        {
            int indexNum = i + 1;
            cachedUsers.Add(
                new CachedUser
                {
                    Id = Guid.Parse($"00000000-0000-0000-0000-0000000000{indexNum + 10:D2}"),
                    UserCode = $"STU{indexNum:D3}",
                    FullName = studentNames[i],
                    Role = "Student",
                    LastUpdated = DateTime.UtcNow,
                }
            );
        }

        await context.CachedUsers.AddRangeAsync(cachedUsers);
        await context.SaveChangesAsync();

        // 2. Seed CachedSubjects (27 subjects)
        var cachedSubjects = new List<CachedSubject>
        {
            // KHỐI 10
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000010"), Code = "MATH10", Name = "Toán Học 10", GradeLevel = 10, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000011"), Code = "LIT10", Name = "Ngữ Văn 10", GradeLevel = 10, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000012"), Code = "ENG10", Name = "Tiếng Anh 10", GradeLevel = 10, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000013"), Code = "PHYS10", Name = "Vật Lý 10", GradeLevel = 10, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000014"), Code = "CHEM10", Name = "Hóa Học 10", GradeLevel = 10, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000015"), Code = "BIO10", Name = "Sinh Học 10", GradeLevel = 10, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000016"), Code = "HIS10", Name = "Lịch Sử 10", GradeLevel = 10, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000017"), Code = "GEO10", Name = "Địa Lý 10", GradeLevel = 10, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000018"), Code = "IT10", Name = "Tin Học 10", GradeLevel = 10, LastUpdated = DateTime.UtcNow },

            // KHỐI 11
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000020"), Code = "MATH11", Name = "Toán Học 11", GradeLevel = 11, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000021"), Code = "LIT11", Name = "Ngữ Văn 11", GradeLevel = 11, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000022"), Code = "ENG11", Name = "Tiếng Anh 11", GradeLevel = 11, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000023"), Code = "PHYS11", Name = "Vật Lý 11", GradeLevel = 11, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000024"), Code = "CHEM11", Name = "Hóa Học 11", GradeLevel = 11, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000025"), Code = "BIO11", Name = "Sinh Học 11", GradeLevel = 11, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000026"), Code = "HIS11", Name = "Lịch Sử 11", GradeLevel = 11, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000027"), Code = "GEO11", Name = "Địa Lý 11", GradeLevel = 11, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000028"), Code = "IT11", Name = "Tin Học 11", GradeLevel = 11, LastUpdated = DateTime.UtcNow },

            // KHỐI 12
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000030"), Code = "MATH12", Name = "Toán Học 12", GradeLevel = 12, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000031"), Code = "LIT12", Name = "Ngữ Văn 12", GradeLevel = 12, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000032"), Code = "ENG12", Name = "Tiếng Anh 12", GradeLevel = 12, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000033"), Code = "PHYS12", Name = "Vật Lý 12", GradeLevel = 12, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000034"), Code = "CHEM12", Name = "Hóa Học 12", GradeLevel = 12, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000035"), Code = "BIO12", Name = "Sinh Học 12", GradeLevel = 12, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000036"), Code = "HIS12", Name = "Lịch Sử 12", GradeLevel = 12, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000037"), Code = "GEO12", Name = "Địa Lý 12", GradeLevel = 12, LastUpdated = DateTime.UtcNow },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000038"), Code = "IT12", Name = "Tin Học 12", GradeLevel = 12, LastUpdated = DateTime.UtcNow },
        };

        await context.CachedSubjects.AddRangeAsync(cachedSubjects);
        await context.SaveChangesAsync();

        // 3. Seed Scores (120 records for 20 students, each having Oral, Midterm, Final in MATH and LIT)
        var scores = new List<Score>();
        ScoreType[] types = { ScoreType.Oral, ScoreType.MidTerm, ScoreType.Final };

        var teaAn = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var teaBinh = Guid.Parse("00000000-0000-0000-0000-000000000003");

        for (int i = 0; i < 20; i++)
        {
            var studentId = Guid.Parse($"00000000-0000-0000-0000-0000000000{i + 11:D2}");

            // Determine subjects and teachers based on grade level
            Guid mathSubjectId;
            Guid litSubjectId;
            Guid mathTeacherId = teaAn;
            Guid litTeacherId = teaBinh;

            if (i < 10) // Grade 10
            {
                mathSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000010"); // MATH10
                litSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000011"); // LIT10
            }
            else if (i < 15) // Grade 11
            {
                mathSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000020"); // MATH11
                litSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000021"); // LIT11
            }
            else // Grade 12
            {
                mathSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000030"); // MATH12
                litSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000031"); // LIT12
            }

            // Generate realistic mock scores dynamically
            decimal mathOral = 7.0m + (i % 4) * 0.5m;
            decimal mathMid = 7.5m + (i % 3) * 0.5m;
            decimal mathFinal = 8.0m + (i % 5) * 0.5m;
            if (mathFinal > 10.0m) mathFinal = 10.0m;

            decimal litOral = 7.5m + (i % 3) * 0.5m;
            decimal litMid = 7.0m + (i % 5) * 0.5m;
            decimal litFinal = 7.5m + (i % 4) * 0.5m;
            if (litFinal > 10.0m) litFinal = 10.0m;

            decimal[] mathVals = { mathOral, mathMid, mathFinal };
            decimal[] litVals = { litOral, litMid, litFinal };

            for (int t = 0; t < 3; t++)
            {
                scores.Add(
                    new Score
                    {
                        Id = Guid.NewGuid(),
                        StudentId = studentId,
                        SubjectId = mathSubjectId,
                        TeacherId = mathTeacherId,
                        ScoreValue = mathVals[t],
                        Type = types[t],
                        Semester = 1,
                        SchoolYear = "2025-2026",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    }
                );

                scores.Add(
                    new Score
                    {
                        Id = Guid.NewGuid(),
                        StudentId = studentId,
                        SubjectId = litSubjectId,
                        TeacherId = litTeacherId,
                        ScoreValue = litVals[t],
                        Type = types[t],
                        Semester = 1,
                        SchoolYear = "2025-2026",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    }
                );
            }
        }

        await context.Scores.AddRangeAsync(scores);
        await context.SaveChangesAsync();
    }
}
