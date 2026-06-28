using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScoreService.DtOs;
using ScoreService.Entities;
using ScoreService.Repositories;

namespace ScoreService.Services;

public class ScoreService : IScoreService
{
    private readonly IScoreRepository _scoreRepository;

    public ScoreService(IScoreRepository scoreRepository)
    {
        _scoreRepository = scoreRepository;
    }

    public async Task<IEnumerable<ScoreResponse>> GetStudentScoresAsync(Guid studentId)
    {
        var scores = await _scoreRepository.GetScoresByStudentAsync(studentId);
        return scores
            .Select(s => new ScoreResponse
            {
                Id = s.Id,
                StudentId = s.StudentId,
                StudentName = s.Student?.FullName ?? "N/A",
                StudentCode = s.Student?.UserCode ?? "N/A",
                SubjectId = s.SubjectId,
                SubjectName = s.Subject?.Name ?? "N/A",
                SubjectCode = s.Subject?.Code ?? "N/A",
                TeacherId = s.TeacherId,
                ScoreValue = s.ScoreValue,
                Type = s.Type.ToString(),
                Semester = s.Semester,
                SchoolYear = s.SchoolYear,
                CreatedAt = s.CreatedAt,
            })
            .ToList();
    }

    public async Task<ScoreResponse> CreateScoreAsync(CreateScoreRequest request, Guid teacherId)
    {
        // Kiểm tra ràng buộc thực thể tồn tại trong CSDL đệm Local
        var isValid = await _scoreRepository.ValidateEntitiesExistAsync(
            request.StudentId,
            request.SubjectId,
            teacherId
        );
        if (!isValid)
        {
            throw new ArgumentException(
                "Thông tin Học sinh, Giáo viên hoặc Môn học không hợp lệ hoặc không tồn tại trong cache đệm."
            );
        }

        var score = new Score
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            SubjectId = request.SubjectId,
            TeacherId = teacherId,
            ScoreValue = request.ScoreValue,
            Type = request.Type,
            Semester = request.Semester,
            SchoolYear = request.SchoolYear,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _scoreRepository.CreateAsync(score);
        await _scoreRepository.SaveChangesAsync();

        var details = await _scoreRepository.GetByIdAsync(score.Id);
        if (details == null)
        {
            throw new KeyNotFoundException("Không tìm thấy điểm số vừa tạo.");
        }

        return new ScoreResponse
        {
            Id = details.Id,
            StudentId = details.StudentId,
            StudentName = details.Student?.FullName ?? "N/A",
            StudentCode = details.Student?.UserCode ?? "N/A",
            SubjectId = details.SubjectId,
            SubjectName = details.Subject?.Name ?? "N/A",
            SubjectCode = details.Subject?.Code ?? "N/A",
            TeacherId = details.TeacherId,
            ScoreValue = details.ScoreValue,
            Type = details.Type.ToString(),
            Semester = details.Semester,
            SchoolYear = details.SchoolYear,
            CreatedAt = details.CreatedAt,
        };
    }

    public async Task<ScoreResponse> UpdateScoreAsync(Guid id, UpdateScoreRequest request)
    {
        var score = await _scoreRepository.GetByIdAsync(id);
        if (score == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy điểm số với Id: {id}");
        }

        score.ScoreValue = request.ScoreValue;
        score.UpdatedAt = DateTime.UtcNow;

        await _scoreRepository.UpdateAsync(score);
        await _scoreRepository.SaveChangesAsync();

        var details = await _scoreRepository.GetByIdAsync(score.Id);
        if (details == null)
        {
            throw new KeyNotFoundException(
                $"Không thể tải lại thông tin điểm số sau khi cập nhật với Id: {id}"
            );
        }

        return new ScoreResponse
        {
            Id = details.Id,
            StudentId = details.StudentId,
            StudentName = details.Student?.FullName ?? "N/A",
            StudentCode = details.Student?.UserCode ?? "N/A",
            SubjectId = details.SubjectId,
            SubjectName = details.Subject?.Name ?? "N/A",
            SubjectCode = details.Subject?.Code ?? "N/A",
            TeacherId = details.TeacherId,
            ScoreValue = details.ScoreValue,
            Type = details.Type.ToString(),
            Semester = details.Semester,
            SchoolYear = details.SchoolYear,
            CreatedAt = details.CreatedAt,
        };
    }

    public async Task DeleteScoreAsync(Guid id)
    {
        var score = await _scoreRepository.GetByIdAsync(id);
        if (score == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy điểm số với Id: {id}");
        }

        await _scoreRepository.DeleteAsync(score);
        await _scoreRepository.SaveChangesAsync();
    }
}
