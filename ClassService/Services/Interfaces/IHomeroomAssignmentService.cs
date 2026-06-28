using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassService.DTOs.HomeroomAssignments;

namespace ClassService.Services.Interfaces;

public interface IHomeroomAssignmentService
{
    // Thêm giáo viên chủ nhiệm
    Task<HomeroomAssignmentResponseDto> AssignHomeroomAsync(Guid classId, AssignHomeroomDto dto);

    // Đổi giáo viên chủ nhiệm
    Task<HomeroomAssignmentResponseDto> ChangeHomeroomAsync(Guid classId, AssignHomeroomDto dto);

    // Lấy thông tin 1 GVCN
    Task<HomeroomAssignmentResponseDto?> GetCurrentHomeroomAsync(Guid classId, string schoolYear);

    // Lấy danh sách GVCN của 1 giáo viên
    Task<IEnumerable<HomeroomAssignmentResponseDto>> GetTeacherHomeroomHistoryAsync(Guid teacherId);

    // Lấy tất cả phân công GVCN theo năm học
    Task<IEnumerable<HomeroomAssignmentResponseDto>> GetAllHomeroomsAsync(string? schoolYear);
}

