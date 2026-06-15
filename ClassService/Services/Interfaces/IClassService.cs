using ClassService.DTOs.Classes;

namespace ClassService.Services.Interfaces;

public interface IClassService
{
    // Lấy tất cả các lớp
    Task<IEnumerable<ClassResponseDto>> GetAllAsync();

    // Lấy lớp theo ID
    Task<ClassResponseDto?> GetByIdAsync(Guid id);

    // Tạo lớp
    Task<ClassResponseDto> CreateAsync(CreateClassDto dto);
    
    // Cập nhật lớp
    Task<ClassResponseDto?> UpdateAsync(Guid id, UpdateClassDto dto);
    
    // Xóa lớp
    Task<ClassResponseDto?> DeleteAsync(Guid id);
}