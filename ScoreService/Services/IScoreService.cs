using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScoreService.DtOs;

namespace ScoreService.Services;

public interface IScoreService
{
    Task<IEnumerable<ScoreResponse>> GetStudentScoresAsync(Guid studentId);
    Task<ScoreResponse> CreateScoreAsync(CreateScoreRequest request, Guid teacherId);
    Task<ScoreResponse> UpdateScoreAsync(Guid id, UpdateScoreRequest request);
    Task DeleteScoreAsync(Guid id);
}
