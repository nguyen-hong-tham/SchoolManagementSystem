using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ScoreService.Data;
using ScoreService.Entities;
using Shared.Events;

namespace ScoreService.Consumers;

public class SubjectCreatedConsumer : IConsumer<SubjectCreatedEvent>
{
    private readonly ScoreDbContext _db;

    public SubjectCreatedConsumer(ScoreDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<SubjectCreatedEvent> context)
    {
        var data = context.Message;
        var exists = await _db.CachedSubjects.AnyAsync(s => s.Id == data.Id);
        if (exists)
            return;

        var cached = new CachedSubject
        {
            Id = data.Id,
            Code = data.Code,
            Name = data.Name,
            GradeLevel = data.GradeLevel,
            LastUpdated = DateTime.UtcNow,
        };
        _db.CachedSubjects.Add(cached);
        await _db.SaveChangesAsync();
    }
}

public class SubjectUpdatedConsumer : IConsumer<SubjectUpdatedEvent>
{
    private readonly ScoreDbContext _db;

    public SubjectUpdatedConsumer(ScoreDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<SubjectUpdatedEvent> context)
    {
        var data = context.Message;
        var subject = await _db.CachedSubjects.FindAsync(data.Id);
        if (subject != null)
        {
            subject.Code = data.Code;
            subject.Name = data.Name;
            subject.GradeLevel = data.GradeLevel;
            await _db.SaveChangesAsync();
        }
    }
}

public class SubjectDeletedConsumer : IConsumer<SubjectDeletedEvent>
{
    private readonly ScoreDbContext _db;

    public SubjectDeletedConsumer(ScoreDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<SubjectDeletedEvent> context)
    {
        var data = context.Message;
        var subject = await _db.CachedSubjects.FindAsync(data.Id);
        if (subject != null)
        {
            _db.CachedSubjects.Remove(subject);
            await _db.SaveChangesAsync();
        }
    }
}
