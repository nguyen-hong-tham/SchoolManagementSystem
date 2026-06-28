using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SubjectService.DTOs;

namespace SubjectService.Services;

public interface ISubjectService
{
    Task<IEnumerable<SubjectResponse>> GetAllSubjectsAsync(int? gradeLevel);
    Task<SubjectResponse> GetSubjectByIdAsync(Guid id);
    Task<SubjectResponse> CreateSubjectAsync(CreateSubjectRequest request);
    Task<SubjectResponse> UpdateSubjectAsync(Guid id, UpdateSubjectRequest request);
    Task DeleteSubjectAsync(Guid id);
}
