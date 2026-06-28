using System;
using System.Collections.Generic;
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

        // Clear existing records to ensure fresh seed
        context.Schedules.RemoveRange(context.Schedules);
        context.TeachingAssignments.RemoveRange(context.TeachingAssignments);
        context.HomeroomAssignments.RemoveRange(context.HomeroomAssignments);
        context.StudentClasses.RemoveRange(context.StudentClasses);
        context.Classes.RemoveRange(context.Classes);
        context.CachedSubjects.RemoveRange(context.CachedSubjects);
        context.CachedUsers.RemoveRange(context.CachedUsers);
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
            "Lê Minh Thư",
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
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                Code = "MATH10",
                Name = "Toán Học 10",
                GradeLevel = 10,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
                Code = "LIT10",
                Name = "Ngữ Văn 10",
                GradeLevel = 10,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                Code = "ENG10",
                Name = "Tiếng Anh 10",
                GradeLevel = 10,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000013"),
                Code = "PHYS10",
                Name = "Vật Lý 10",
                GradeLevel = 10,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000014"),
                Code = "CHEM10",
                Name = "Hóa Học 10",
                GradeLevel = 10,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000015"),
                Code = "BIO10",
                Name = "Sinh Học 10",
                GradeLevel = 10,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000016"),
                Code = "HIS10",
                Name = "Lịch Sử 10",
                GradeLevel = 10,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000017"),
                Code = "GEO10",
                Name = "Địa Lý 10",
                GradeLevel = 10,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000018"),
                Code = "IT10",
                Name = "Tin Học 10",
                GradeLevel = 10,
                LastUpdated = DateTime.UtcNow,
            },
            // KHỐI 11
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000020"),
                Code = "MATH11",
                Name = "Toán Học 11",
                GradeLevel = 11,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000021"),
                Code = "LIT11",
                Name = "Ngữ Văn 11",
                GradeLevel = 11,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000022"),
                Code = "ENG11",
                Name = "Tiếng Anh 11",
                GradeLevel = 11,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000023"),
                Code = "PHYS11",
                Name = "Vật Lý 11",
                GradeLevel = 11,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000024"),
                Code = "CHEM11",
                Name = "Hóa Học 11",
                GradeLevel = 11,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000025"),
                Code = "BIO11",
                Name = "Sinh Học 11",
                GradeLevel = 11,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000026"),
                Code = "HIS11",
                Name = "Lịch Sử 11",
                GradeLevel = 11,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000027"),
                Code = "GEO11",
                Name = "Địa Lý 11",
                GradeLevel = 11,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000028"),
                Code = "IT11",
                Name = "Tin Học 11",
                GradeLevel = 11,
                LastUpdated = DateTime.UtcNow,
            },
            // KHỐI 12
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000030"),
                Code = "MATH12",
                Name = "Toán Học 12",
                GradeLevel = 12,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000031"),
                Code = "LIT12",
                Name = "Ngữ Văn 12",
                GradeLevel = 12,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000032"),
                Code = "ENG12",
                Name = "Tiếng Anh 12",
                GradeLevel = 12,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000033"),
                Code = "PHYS12",
                Name = "Vật Lý 12",
                GradeLevel = 12,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000034"),
                Code = "CHEM12",
                Name = "Hóa Học 12",
                GradeLevel = 12,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000035"),
                Code = "BIO12",
                Name = "Sinh Học 12",
                GradeLevel = 12,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000036"),
                Code = "HIS12",
                Name = "Lịch Sử 12",
                GradeLevel = 12,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000037"),
                Code = "GEO12",
                Name = "Địa Lý 12",
                GradeLevel = 12,
                LastUpdated = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000038"),
                Code = "IT12",
                Name = "Tin Học 12",
                GradeLevel = 12,
                LastUpdated = DateTime.UtcNow,
            },
        };

        await context.CachedSubjects.AddRangeAsync(cachedSubjects);
        await context.SaveChangesAsync();

        // 3. Seed Classes (20 classes)
        var classes = new List<Class>();
        // Grade 10: 10A1 to 10A7
        for (int i = 1; i <= 7; i++)
        {
            classes.Add(
                new Class
                {
                    Id = Guid.Parse($"00000000-0000-0000-0000-0000000101{i:D2}"),
                    Name = $"10A{i}",
                    GradeLevel = 10,
                    SchoolYear = "2025-2026",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                }
            );
        }
        // Grade 11: 11A1 to 11A7
        for (int i = 1; i <= 7; i++)
        {
            classes.Add(
                new Class
                {
                    Id = Guid.Parse($"00000000-0000-0000-0000-0000000111{i:D2}"),
                    Name = $"11A{i}",
                    GradeLevel = 11,
                    SchoolYear = "2025-2026",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                }
            );
        }
        // Grade 12: 12A1 to 12A6
        for (int i = 1; i <= 6; i++)
        {
            classes.Add(
                new Class
                {
                    Id = Guid.Parse($"00000000-0000-0000-0000-0000000121{i:D2}"),
                    Name = $"12A{i}",
                    GradeLevel = 12,
                    SchoolYear = "2025-2026",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                }
            );
        }

        // Grade 11 for 2026-2027
        for (int i = 1; i <= 7; i++)
        {
            classes.Add(
                new Class
                {
                    Id = Guid.Parse($"00000000-0000-0000-0000-0000000112{i:D2}"),
                    Name = $"11A{i}",
                    GradeLevel = 11,
                    SchoolYear = "2026-2027",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                }
            );
        }
        // Grade 12 for 2026-2027
        for (int i = 1; i <= 6; i++)
        {
            classes.Add(
                new Class
                {
                    Id = Guid.Parse($"00000000-0000-0000-0000-0000000122{i:D2}"),
                    Name = $"12A{i}",
                    GradeLevel = 12,
                    SchoolYear = "2026-2027",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                }
            );
        }

        await context.Classes.AddRangeAsync(classes);
        await context.SaveChangesAsync();

        var class1Id = Guid.Parse("00000000-0000-0000-0000-000000010101"); // 10A1
        var class2Id = Guid.Parse("00000000-0000-0000-0000-000000010102"); // 10A2
        var class3Id = Guid.Parse("00000000-0000-0000-0000-000000011101"); // 11A1
        var class4Id = Guid.Parse("00000000-0000-0000-0000-000000012101"); // 12A1

        // 4. Seed StudentClass (20 students assigned to classes)
        var studentClasses = new List<StudentClass>();
        // STU001 - STU005 (class1: 10A1)
        for (int i = 0; i < 5; i++)
        {
            studentClasses.Add(
                new StudentClass
                {
                    Id = Guid.NewGuid(),
                    StudentId = Guid.Parse($"00000000-0000-0000-0000-0000000000{i + 11:D2}"),
                    ClassId = class1Id,
                    SchoolYear = "2025-2026",
                    AssignedDate = DateTime.UtcNow,
                    IsCurrent = true,
                }
            );
        }
        // STU006 - STU010 (class2: 10A2)
        for (int i = 5; i < 10; i++)
        {
            studentClasses.Add(
                new StudentClass
                {
                    Id = Guid.NewGuid(),
                    StudentId = Guid.Parse($"00000000-0000-0000-0000-0000000000{i + 11:D2}"),
                    ClassId = class2Id,
                    SchoolYear = "2025-2026",
                    AssignedDate = DateTime.UtcNow,
                    IsCurrent = true,
                }
            );
        }
        // STU011 - STU015 (class3: 11A1)
        for (int i = 10; i < 15; i++)
        {
            studentClasses.Add(
                new StudentClass
                {
                    Id = Guid.NewGuid(),
                    StudentId = Guid.Parse($"00000000-0000-0000-0000-0000000000{i + 11:D2}"),
                    ClassId = class3Id,
                    SchoolYear = "2025-2026",
                    AssignedDate = DateTime.UtcNow,
                    IsCurrent = true,
                }
            );
        }
        // STU016 - STU020 (class4: 12A1)
        for (int i = 15; i < 20; i++)
        {
            studentClasses.Add(
                new StudentClass
                {
                    Id = Guid.NewGuid(),
                    StudentId = Guid.Parse($"00000000-0000-0000-0000-0000000000{i + 11:D2}"),
                    ClassId = class4Id,
                    SchoolYear = "2025-2026",
                    AssignedDate = DateTime.UtcNow,
                    IsCurrent = true,
                }
            );
        }

        await context.StudentClasses.AddRangeAsync(studentClasses);
        await context.SaveChangesAsync();

        // 5. Seed HomeroomAssignment (4 assignments)
        var homerooms = new List<HomeroomAssignment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = Guid.Parse("00000000-0000-0000-0000-000000000002"), // TEA001
                ClassId = class1Id,
                SchoolYear = "2025-2026",
                AssignedDate = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = Guid.Parse("00000000-0000-0000-0000-000000000003"), // TEA002
                ClassId = class2Id,
                SchoolYear = "2025-2026",
                AssignedDate = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = Guid.Parse("00000000-0000-0000-0000-000000000004"), // TEA003
                ClassId = class3Id,
                SchoolYear = "2025-2026",
                AssignedDate = DateTime.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = Guid.Parse("00000000-0000-0000-0000-000000000005"), // TEA004
                ClassId = class4Id,
                SchoolYear = "2025-2026",
                AssignedDate = DateTime.UtcNow,
            },
        };

        await context.HomeroomAssignments.AddRangeAsync(homerooms);
        await context.SaveChangesAsync();

        // 6. Seed TeachingAssignment (23 assignments)
        var math10 = Guid.Parse("00000000-0000-0000-0000-000000000010");
        var lit10 = Guid.Parse("00000000-0000-0000-0000-000000000011");
        var eng10 = Guid.Parse("00000000-0000-0000-0000-000000000012");
        var phys10 = Guid.Parse("00000000-0000-0000-0000-000000000013");

        var math11 = Guid.Parse("00000000-0000-0000-0000-000000000020");
        var lit11 = Guid.Parse("00000000-0000-0000-0000-000000000021");
        var eng11 = Guid.Parse("00000000-0000-0000-0000-000000000022");
        var phys11 = Guid.Parse("00000000-0000-0000-0000-000000000023");

        var math12 = Guid.Parse("00000000-0000-0000-0000-000000000030");
        var lit12 = Guid.Parse("00000000-0000-0000-0000-000000000031");
        var eng12 = Guid.Parse("00000000-0000-0000-0000-000000000032");
        var phys12 = Guid.Parse("00000000-0000-0000-0000-000000000033");

        var teaAn = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var teaBinh = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var teaChinh = Guid.Parse("00000000-0000-0000-0000-000000000004");
        var teaDung = Guid.Parse("00000000-0000-0000-0000-000000000005");

        var teaching = new List<TeachingAssignment>();

        // Teacher An teaches MATH10 in 10A1 to 10A4, MATH11 in 11A1 to 11A3, MATH12 in 12A1 and 12A2 (9 assignments)
        for (int i = 1; i <= 4; i++)
        {
            teaching.Add(
                new()
                {
                    Id = Guid.NewGuid(),
                    TeacherId = teaAn,
                    SubjectId = math10,
                    ClassId = Guid.Parse($"00000000-0000-0000-0000-0000000101{i:D2}"),
                    SchoolYear = "2025-2026",
                }
            );
        }
        for (int i = 1; i <= 3; i++)
        {
            teaching.Add(
                new()
                {
                    Id = Guid.NewGuid(),
                    TeacherId = teaAn,
                    SubjectId = math11,
                    ClassId = Guid.Parse($"00000000-0000-0000-0000-0000000111{i:D2}"),
                    SchoolYear = "2025-2026",
                }
            );
        }
        for (int i = 1; i <= 2; i++)
        {
            teaching.Add(
                new()
                {
                    Id = Guid.NewGuid(),
                    TeacherId = teaAn,
                    SubjectId = math12,
                    ClassId = Guid.Parse($"00000000-0000-0000-0000-0000000121{i:D2}"),
                    SchoolYear = "2025-2026",
                }
            );
        }

        // Teacher Binh teaches LIT10 in 10A1 & 10A2, LIT11 in 11A1, LIT12 in 12A1 (4 assignments)
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaBinh,
                SubjectId = lit10,
                ClassId = class1Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaBinh,
                SubjectId = lit10,
                ClassId = class2Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaBinh,
                SubjectId = lit11,
                ClassId = class3Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaBinh,
                SubjectId = lit12,
                ClassId = class4Id,
                SchoolYear = "2025-2026",
            }
        );

        // Teacher Chinh teaches ENG10 in 10A1 to 10A3, ENG11 in 11A1 & 11A2, ENG12 in 12A1 & 12A2 (7 assignments)
        for (int i = 1; i <= 3; i++)
        {
            teaching.Add(
                new()
                {
                    Id = Guid.NewGuid(),
                    TeacherId = teaChinh,
                    SubjectId = eng10,
                    ClassId = Guid.Parse($"00000000-0000-0000-0000-0000000101{i:D2}"),
                    SchoolYear = "2025-2026",
                }
            );
        }
        for (int i = 1; i <= 2; i++)
        {
            teaching.Add(
                new()
                {
                    Id = Guid.NewGuid(),
                    TeacherId = teaChinh,
                    SubjectId = eng11,
                    ClassId = Guid.Parse($"00000000-0000-0000-0000-0000000111{i:D2}"),
                    SchoolYear = "2025-2026",
                }
            );
        }
        for (int i = 1; i <= 2; i++)
        {
            teaching.Add(
                new()
                {
                    Id = Guid.NewGuid(),
                    TeacherId = teaChinh,
                    SubjectId = eng12,
                    ClassId = Guid.Parse($"00000000-0000-0000-0000-0000000121{i:D2}"),
                    SchoolYear = "2025-2026",
                }
            );
        }

        // Teacher Dung teaches PHYS10 in 10A1, PHYS11 in 11A1, PHYS12 in 12A1 (3 assignments)
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaDung,
                SubjectId = phys10,
                ClassId = class1Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaDung,
                SubjectId = phys11,
                ClassId = class3Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaDung,
                SubjectId = phys12,
                ClassId = class4Id,
                SchoolYear = "2025-2026",
            }
        );

        // Seed remaining teachers TEA005 to TEA009 teaching assignments
        var chem10 = Guid.Parse("00000000-0000-0000-0000-000000000014");
        var bio10 = Guid.Parse("00000000-0000-0000-0000-000000000015");
        var his10 = Guid.Parse("00000000-0000-0000-0000-000000000016");
        var geo10 = Guid.Parse("00000000-0000-0000-0000-000000000017");
        var it10 = Guid.Parse("00000000-0000-0000-0000-000000000018");

        var chem11 = Guid.Parse("00000000-0000-0000-0000-000000000024");
        var bio11 = Guid.Parse("00000000-0000-0000-0000-000000000025");
        var his11 = Guid.Parse("00000000-0000-0000-0000-000000000026");
        var geo11 = Guid.Parse("00000000-0000-0000-0000-000000000027");
        var it11 = Guid.Parse("00000000-0000-0000-0000-000000000028");

        var chem12 = Guid.Parse("00000000-0000-0000-0000-000000000034");
        var bio12 = Guid.Parse("00000000-0000-0000-0000-000000000035");
        var his12 = Guid.Parse("00000000-0000-0000-0000-000000000036");
        var geo12 = Guid.Parse("00000000-0000-0000-0000-000000000037");
        var it12 = Guid.Parse("00000000-0000-0000-0000-000000000038");

        var teaChi = Guid.Parse("00000000-0000-0000-0000-000000000006");
        var teaGiang = Guid.Parse("00000000-0000-0000-0000-000000000007");
        var teaHanh = Guid.Parse("00000000-0000-0000-0000-000000000008");
        var teaHai = Guid.Parse("00000000-0000-0000-0000-000000000009");
        var teaTuan = Guid.Parse("00000000-0000-0000-0000-000000000010");

        // Chemistry - Hóa học (TEA005)
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaChi,
                SubjectId = chem10,
                ClassId = class1Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaChi,
                SubjectId = chem11,
                ClassId = class3Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaChi,
                SubjectId = chem12,
                ClassId = class4Id,
                SchoolYear = "2025-2026",
            }
        );

        // Biology - Sinh học (TEA006)
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaGiang,
                SubjectId = bio10,
                ClassId = class1Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaGiang,
                SubjectId = bio11,
                ClassId = class3Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaGiang,
                SubjectId = bio12,
                ClassId = class4Id,
                SchoolYear = "2025-2026",
            }
        );

        // History - Lịch sử (TEA007)
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaHanh,
                SubjectId = his10,
                ClassId = class1Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaHanh,
                SubjectId = his11,
                ClassId = class3Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaHanh,
                SubjectId = his12,
                ClassId = class4Id,
                SchoolYear = "2025-2026",
            }
        );

        // Geography - Địa lý (TEA008)
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaHai,
                SubjectId = geo10,
                ClassId = class1Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaHai,
                SubjectId = geo11,
                ClassId = class3Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaHai,
                SubjectId = geo12,
                ClassId = class4Id,
                SchoolYear = "2025-2026",
            }
        );

        // IT - Tin học (TEA009)
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaTuan,
                SubjectId = it10,
                ClassId = class1Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaTuan,
                SubjectId = it11,
                ClassId = class3Id,
                SchoolYear = "2025-2026",
            }
        );
        teaching.Add(
            new()
            {
                Id = Guid.NewGuid(),
                TeacherId = teaTuan,
                SubjectId = it12,
                ClassId = class4Id,
                SchoolYear = "2025-2026",
            }
        );

        await context.TeachingAssignments.AddRangeAsync(teaching);
        await context.SaveChangesAsync();

        // 7. Seed Schedule (26 schedules)
        var schedules = new List<Schedule>
        {
            // Class 10A1 (11 schedules)
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = math10,
                TeacherId = teaAn,
                DayOfWeek = 2,
                Period = 1,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = lit10,
                TeacherId = teaBinh,
                DayOfWeek = 2,
                Period = 2,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = eng10,
                TeacherId = teaChinh,
                DayOfWeek = 3,
                Period = 1,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = chem10,
                TeacherId = teaChi,
                DayOfWeek = 3,
                Period = 3,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = bio10,
                TeacherId = teaGiang,
                DayOfWeek = 3,
                Period = 4,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = his10,
                TeacherId = teaHanh,
                DayOfWeek = 4,
                Period = 1,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = geo10,
                TeacherId = teaHai,
                DayOfWeek = 4,
                Period = 2,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = phys10,
                TeacherId = teaDung,
                DayOfWeek = 4,
                Period = 3,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = it10,
                TeacherId = teaTuan,
                DayOfWeek = 4,
                Period = 5,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = math10,
                TeacherId = teaAn,
                DayOfWeek = 5,
                Period = 1,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class1Id,
                SubjectId = eng10,
                TeacherId = teaChinh,
                DayOfWeek = 5,
                Period = 2,
                Room = "Phòng 101",
                SchoolYear = "2025-2026",
            },
            // Class 10A2 (5 schedules)
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class2Id,
                SubjectId = math10,
                TeacherId = teaAn,
                DayOfWeek = 2,
                Period = 3,
                Room = "Phòng 102",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class2Id,
                SubjectId = lit10,
                TeacherId = teaBinh,
                DayOfWeek = 3,
                Period = 1,
                Room = "Phòng 102",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class2Id,
                SubjectId = eng10,
                TeacherId = teaChinh,
                DayOfWeek = 3,
                Period = 2,
                Room = "Phòng 102",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class2Id,
                SubjectId = math10,
                TeacherId = teaAn,
                DayOfWeek = 4,
                Period = 1,
                Room = "Phòng 102",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class2Id,
                SubjectId = lit10,
                TeacherId = teaBinh,
                DayOfWeek = 5,
                Period = 3,
                Room = "Phòng 102",
                SchoolYear = "2025-2026",
            },
            // Class 11A1 (5 schedules)
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class3Id,
                SubjectId = math11,
                TeacherId = teaAn,
                DayOfWeek = 2,
                Period = 4,
                Room = "Phòng 201",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class3Id,
                SubjectId = lit11,
                TeacherId = teaBinh,
                DayOfWeek = 3,
                Period = 3,
                Room = "Phòng 201",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class3Id,
                SubjectId = eng11,
                TeacherId = teaChinh,
                DayOfWeek = 4,
                Period = 2,
                Room = "Phòng 201",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class3Id,
                SubjectId = phys11,
                TeacherId = teaDung,
                DayOfWeek = 4,
                Period = 4,
                Room = "Phòng 201",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class3Id,
                SubjectId = math11,
                TeacherId = teaAn,
                DayOfWeek = 5,
                Period = 4,
                Room = "Phòng 201",
                SchoolYear = "2025-2026",
            },
            // Class 12A1 (5 schedules)
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class4Id,
                SubjectId = math12,
                TeacherId = teaAn,
                DayOfWeek = 2,
                Period = 5,
                Room = "Phòng 301",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class4Id,
                SubjectId = lit12,
                TeacherId = teaBinh,
                DayOfWeek = 3,
                Period = 4,
                Room = "Phòng 301",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class4Id,
                SubjectId = eng12,
                TeacherId = teaChinh,
                DayOfWeek = 3,
                Period = 5,
                Room = "Phòng 301",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class4Id,
                SubjectId = phys12,
                TeacherId = teaDung,
                DayOfWeek = 5,
                Period = 1,
                Room = "Phòng 301",
                SchoolYear = "2025-2026",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClassId = class4Id,
                SubjectId = math12,
                TeacherId = teaAn,
                DayOfWeek = 5,
                Period = 5,
                Room = "Phòng 301",
                SchoolYear = "2025-2026",
            },
        };

        await context.Schedules.AddRangeAsync(schedules);
        await context.SaveChangesAsync();
    }
}
