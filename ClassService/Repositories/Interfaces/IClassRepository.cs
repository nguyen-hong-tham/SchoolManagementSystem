using ClassService.Entities;

namespace ClassService.Repositories.Interfaces;

public interface IClassRepository
{
    // Lấy danh sách các lớp học
    Task<List<Class>> GetAllAsync();

    // Lấy lớp học theo ID
    Task<Class?> GetByIdAsync(Guid id);

    // Thêm lớp học
    Task AddAsync(Class entity);

    // Cập nhật lớp học
    void Update(Class entity);

    // Xóa lớp học
    void Delete(Class entity);

    // Lưu thay đổi
    Task SaveChangesAsync();
}
