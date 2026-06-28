using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Shared.Events;
using SubjectService.DTOs;
using SubjectService.Entities;
using SubjectService.Repositories;

namespace SubjectService.Services;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public SubjectService(ISubjectRepository subjectRepository, IPublishEndpoint publishEndpoint)
    {
        _subjectRepository = subjectRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<IEnumerable<SubjectResponse>> GetAllSubjectsAsync(int? gradeLevel)
    {
        var subjects = await _subjectRepository.GetAllSubjectsAsync();

        if (gradeLevel.HasValue)
        {
            subjects = subjects.Where(s => s.GradeLevel == gradeLevel.Value);
        }

        return subjects
            .Select(s => new SubjectResponse
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                GradeLevel = s.GradeLevel,
                CreatedAt = s.CreatedAt,
            })
            .ToList();
    }

    public async Task<SubjectResponse> GetSubjectByIdAsync(Guid id)
    {
        var subject = await _subjectRepository.GetSubjectByIdAsync(id);
        if (subject == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy môn học với Id: {id}");
        }

        return new SubjectResponse
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Description = subject.Description,
            GradeLevel = subject.GradeLevel,
            CreatedAt = subject.CreatedAt,
        };
    }

    public async Task<SubjectResponse> CreateSubjectAsync(CreateSubjectRequest request)
    {
        // Kiểm tra trùng mã môn học (Business Logic)
        var exists = await _subjectRepository.GetSubjectByCodeAsync(request.Code);
        if (exists != null)
        {
            throw new InvalidOperationException("Môn học đã tồn tại");
        }

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Description = request.Description,
            GradeLevel = request.GradeLevel,
            CreatedAt = DateTime.UtcNow,
        };

        await _subjectRepository.CreateSubjectAsync(subject);
        await _subjectRepository.SaveChangesAsync();

        // Phát sự kiện đồng bộ dữ liệu qua RabbitMQ (chạy nền)
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish<SubjectCreatedEvent>(
                    new
                    {
                        Id = subject.Id,
                        Code = subject.Code,
                        Name = subject.Name,
                        GradeLevel = subject.GradeLevel,
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Publish Info] Failed to publish SubjectCreatedEvent in background: {ex.Message}");
            }
        });

        return new SubjectResponse
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Description = subject.Description,
            GradeLevel = subject.GradeLevel,
            CreatedAt = subject.CreatedAt,
        };
    }

    public async Task<SubjectResponse> UpdateSubjectAsync(Guid id, UpdateSubjectRequest request)
    {
        var subject = await _subjectRepository.GetSubjectByIdAsync(id);
        if (subject == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy môn học với Id: {id}");
        }

        subject.Name = request.Name;
        subject.Description = request.Description;
        subject.GradeLevel = request.GradeLevel;

        await _subjectRepository.UpdateSubjectAsync(subject);
        await _subjectRepository.SaveChangesAsync();

        // Phát sự kiện cập nhật qua RabbitMQ (chạy nền)
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish<SubjectUpdatedEvent>(
                    new
                    {
                        Id = subject.Id,
                        Code = subject.Code,
                        Name = subject.Name,
                        GradeLevel = subject.GradeLevel,
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Publish Info] Failed to publish SubjectUpdatedEvent in background: {ex.Message}");
            }
        });

        return new SubjectResponse
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Description = subject.Description,
            GradeLevel = subject.GradeLevel,
            CreatedAt = subject.CreatedAt,
        };
    }

    public async Task DeleteSubjectAsync(Guid id)
    {
        var subject = await _subjectRepository.GetSubjectByIdAsync(id);
        if (subject == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy môn học với Id: {id}");
        }

        await _subjectRepository.DeleteSubjectAsync(id);
        await _subjectRepository.SaveChangesAsync();

        // Phát sự kiện xóa qua RabbitMQ (chạy nền)
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish<SubjectDeletedEvent>(new { Id = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Publish Info] Failed to publish SubjectDeletedEvent in background: {ex.Message}");
            }
        });
    }
}
