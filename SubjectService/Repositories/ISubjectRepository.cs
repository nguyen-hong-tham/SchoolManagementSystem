using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SubjectService.Entities;

namespace SubjectService.Repositories;

public interface ISubjectRepository
{
    Task<IEnumerable<Subject>> GetAllSubjectsAsync();
    Task<Subject?> GetSubjectByIdAsync(Guid id);
    Task<Subject?> GetSubjectByCodeAsync(string code);
    Task<Subject> CreateSubjectAsync(Subject subject);
    Task UpdateSubjectAsync(Subject subject);
    Task DeleteSubjectAsync(Guid id);
    Task SaveChangesAsync();
}
