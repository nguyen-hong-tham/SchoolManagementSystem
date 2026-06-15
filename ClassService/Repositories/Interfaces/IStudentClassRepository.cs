using ClassService.Entities;

namespace ClassService.Repositories.Interfaces;

public interface IStudentClassRepository
{
    Task<List<StudentClass>> GetStudentsByClassIdAsync(Guid classId);

    Task<StudentClass?> GetCurrentStudentClassAsync(Guid studentId);

    Task AddAsync(StudentClass studentClass);

    Task SaveChangesAsync();
}