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
                IsActive = s.IsActive,
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
            IsActive = subject.IsActive,
            CreatedAt = subject.CreatedAt,
        };
    }

    public async Task<bool> CheckCodeExistsAsync(string code, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        var existing = await _subjectRepository.GetSubjectByCodeAsync(code, excludeId);
        return existing != null;
    }

    public async Task<SubjectResponse> CreateSubjectAsync(CreateSubjectRequest request)
    {
        var normalizedCode = request.Code.Trim().ToUpper();

        // Kiểm tra trùng mã môn học (Business Logic)
        var exists = await _subjectRepository.GetSubjectByCodeAsync(normalizedCode);
        if (exists != null)
        {
            throw new InvalidOperationException($"Mã môn học '{normalizedCode}' đã tồn tại trong hệ thống. Vui lòng chọn mã khác.");
        }

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            GradeLevel = request.GradeLevel,
            IsActive = request.IsActive,
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
            IsActive = subject.IsActive,
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

        subject.Name = request.Name.Trim();
        subject.Description = request.Description?.Trim() ?? string.Empty;
        subject.GradeLevel = request.GradeLevel;
        subject.IsActive = request.IsActive;

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
            IsActive = subject.IsActive,
            CreatedAt = subject.CreatedAt,
        };
    }

    public async Task<SubjectResponse> ToggleStatusAsync(Guid id)
    {
        var subject = await _subjectRepository.GetSubjectByIdAsync(id);
        if (subject == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy môn học với Id: {id}");
        }

        subject.IsActive = !subject.IsActive;
        await _subjectRepository.UpdateSubjectAsync(subject);
        await _subjectRepository.SaveChangesAsync();

        return new SubjectResponse
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Description = subject.Description,
            GradeLevel = subject.GradeLevel,
            IsActive = subject.IsActive,
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
