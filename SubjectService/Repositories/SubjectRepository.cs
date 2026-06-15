using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubjectService.Data;
using SubjectService.Entities;

namespace SubjectService.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly SubjectDbContext _db;

    public SubjectRepository(SubjectDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Subject>> GetAllSubjectsAsync()
    {
        return await _db.Subjects.ToListAsync();
    }

    public async Task<Subject?> GetSubjectByIdAsync(Guid id)
    {
        return await _db.Subjects.FindAsync(id);
    }

    public async Task<Subject?> GetSubjectByCodeAsync(string code)
    {
        return await _db.Subjects.FirstOrDefaultAsync(s => s.Code == code);
    }

    public async Task<Subject> CreateSubjectAsync(Subject subject)
    {
        await _db.Subjects.AddAsync(subject);
        return subject;
    }

    public Task UpdateSubjectAsync(Subject subject)
    {
        // Entity Framework Core theo dõi (track) sự thay đổi của thực thể trong bộ nhớ.
        // Hàm Update() chỉ đánh dấu thực thể là Modified, không ghi dữ liệu ngay.
        _db.Subjects.Update(subject);
        return Task.CompletedTask;
    }

    public async Task DeleteSubjectAsync(Guid id)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject != null)
        {
            _db.Subjects.Remove(subject);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
