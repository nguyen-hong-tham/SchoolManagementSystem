using System;
using System.Threading.Tasks;
using ClassService.Data;
using ClassService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace ClassService.Consumers;

public class SubjectCreatedConsumer : IConsumer<SubjectCreatedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public SubjectCreatedConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<SubjectCreatedEvent> context)
    {
        var data = context.Message;

        var existing = await _dbContext.CachedSubjects.AnyAsync(s => s.Id == data.Id);
        if (existing)
            return;

        var cachedSubject = new CachedSubject
        {
            Id = data.Id,
            Code = data.Code,
            Name = data.Name,
            GradeLevel = data.GradeLevel,
            LastUpdated = DateTime.UtcNow
        };

        _dbContext.CachedSubjects.Add(cachedSubject);
        await _dbContext.SaveChangesAsync();
    }
}

public class SubjectUpdatedConsumer : IConsumer<SubjectUpdatedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public SubjectUpdatedConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<SubjectUpdatedEvent> context)
    {
        var data = context.Message;
        var subject = await _dbContext.CachedSubjects.FirstOrDefaultAsync(s => s.Id == data.Id);

        if (subject != null)
        {
            subject.Code = data.Code;
            subject.Name = data.Name;
            subject.GradeLevel = data.GradeLevel;
            subject.LastUpdated = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }
    }
}

public class SubjectDeletedConsumer : IConsumer<SubjectDeletedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public SubjectDeletedConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<SubjectDeletedEvent> context)
    {
        var data = context.Message;
        var subject = await _dbContext.CachedSubjects.FirstOrDefaultAsync(s => s.Id == data.Id);

        if (subject != null)
        {
            _dbContext.CachedSubjects.Remove(subject);
            await _dbContext.SaveChangesAsync();
        }
    }
}
