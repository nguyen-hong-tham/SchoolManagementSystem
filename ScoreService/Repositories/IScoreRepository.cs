using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ScoreService.Entities;

namespace ScoreService.Repositories;

public interface IScoreRepository
{
    Task<IEnumerable<Score>> GetScoresByStudentAsync(Guid studentId);
    Task<IEnumerable<Score>> GetScoresByClassAndSubjectAsync(
        List<Guid> studentIds,
        Guid subjectId,
        string schoolYear,
        int semester
    );
    Task<Score?> GetByIdAsync(Guid id);
    Task<Score> CreateAsync(Score score);
    Task UpdateAsync(Score score);
    Task DeleteAsync(Score score);
    Task<bool> ValidateEntitiesExistAsync(Guid studentId, Guid subjectId, Guid teacherId);
    Task SaveChangesAsync();
}
