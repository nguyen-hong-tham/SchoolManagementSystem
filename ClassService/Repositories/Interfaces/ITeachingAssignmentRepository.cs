using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassService.Entities;

namespace ClassService.Repositories.Interfaces;

public interface ITeachingAssignmentRepository
{
    Task<List<TeachingAssignment>> GetAssignmentsByClassAsync(Guid classId, string schoolYear);
    Task<List<TeachingAssignment>> GetAssignmentsByTeacherAsync(Guid teacherId, string schoolYear);
    Task<TeachingAssignment?> GetAssignmentAsync(Guid classId, Guid subjectId, string schoolYear);
    Task AddAsync(TeachingAssignment entity);
    void Update(TeachingAssignment entity);
    Task SaveChangesAsync();
}
