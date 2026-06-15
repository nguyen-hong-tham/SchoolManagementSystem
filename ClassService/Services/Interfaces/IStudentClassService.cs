using ClassService.DTOs.StudentClasses;

namespace ClassService.Services.Interfaces;

public interface IStudentClassService
{
    // Thêm học sinh vào lớp
    Task<StudentClassResponseDto> AssignStudentAsync(Guid classId, AssignStudentDto dto);

    // Xem danh sách học sinh của lớp
    Task<IEnumerable<StudentClassResponseDto>> GetStudentsByClassIdAsync(Guid classId);

    // Xem lớp hiện tại của học sinh
    Task<StudentClassResponseDto?> GetCurrentClassAsync(Guid studentId);

    // Chuyển lớp
    Task<StudentClassResponseDto> TransferStudentAsync(Guid studentId, TransferStudentDto dto);

    // Lên lớp
    Task<StudentClassResponseDto> PromoteStudentAsync(Guid studentId, PromoteStudentDto dto);

    // Xóa học sinh khỏi lớp
    Task<bool> RemoveStudentAsync(Guid classId, Guid studentId);
}
