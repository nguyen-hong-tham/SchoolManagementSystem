using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ScoreService.Data;
using ScoreService.Entities;

namespace ScoreService.Repositories;

public class ScoreRepository : IScoreRepository
{
    private readonly ScoreDbContext _db;

    public ScoreRepository(ScoreDbContext db)
    {
        _db = db;
    }

    // Lấy tất cả điểm của 1 học sinh
    public async Task<IEnumerable<Score>> GetScoresByStudentAsync(Guid studentId)
    {
        return await _db
            .Scores.Include(s => s.Student)
            .Include(s => s.Subject)
            .Where(s => s.StudentId == studentId)
            .ToListAsync();
    }

    // Lấy điểm của một nhóm học sinh theo môn học, năm học và học kỳ.
    public async Task<IEnumerable<Score>> GetScoresByClassAndSubjectAsync(
        List<Guid> studentIds,
        Guid subjectId,
        string schoolYear,
        int semester
    )
    {
        return await _db
            .Scores.Include(s => s.Student)
            .Include(s => s.Subject)
            .Where(s =>
                studentIds.Contains(s.StudentId)
                && s.SubjectId == subjectId
                && s.SchoolYear == schoolYear
                && s.Semester == semester
            )
            .ToListAsync();
    }

    // Tìm một bản ghi điểm theo ID.
    public async Task<Score?> GetByIdAsync(Guid id)
    {
        return await _db
            .Scores.Include(s => s.Student)
            .Include(s => s.Subject)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    // Thêm một bản ghi điểm mới vào database.
    public async Task<Score> CreateAsync(Score score)
    {
        await _db.Scores.AddAsync(score);
        return score;
    }

    // Cập nhật một bản ghi điểm.
    public Task UpdateAsync(Score score)
    {
        _db.Scores.Update(score);
        return Task.CompletedTask;
    }

    // Xoá một bản ghi điểm.
    public Task DeleteAsync(Score score)
    {
        _db.Scores.Remove(score);
        return Task.CompletedTask;
    }

    // Kiểm tra sự tồn tại của các thực thể liên quan (để đảm bảo tính toàn vẹn dữ liệu).
    public async Task<bool> ValidateEntitiesExistAsync(
        Guid studentId,
        Guid subjectId,
        Guid teacherId
    )
    {
        var studentExists = await _db.CachedUsers.AnyAsync(u =>
            u.Id == studentId && u.Role == "Student"
        );
        var subjectExists = await _db.CachedSubjects.AnyAsync(s => s.Id == subjectId);
        var teacherExists = await _db.CachedUsers.AnyAsync(t =>
            t.Id == teacherId && t.Role == "Teacher"
        );
        return studentExists && subjectExists && teacherExists;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
