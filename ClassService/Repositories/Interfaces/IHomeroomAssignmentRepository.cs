using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassService.Entities;

namespace ClassService.Repositories.Interfaces;

public interface IHomeroomAssignmentRepository
{
    // Tìm giáo viên chủ nhiệm hiện tại của lớp theo năm học  
    Task<HomeroomAssignment?> GetActiveAssignmentByClassAsync(Guid classId, string schoolYear);

    // Xem lịch sử phân công của 1 giáo viên
    Task<List<HomeroomAssignment>> GetHistoryByTeacherAsync(Guid teacherId);

    Task AddAsync(HomeroomAssignment entity);

    void Update(HomeroomAssignment entity);

    Task SaveChangesAsync();
}