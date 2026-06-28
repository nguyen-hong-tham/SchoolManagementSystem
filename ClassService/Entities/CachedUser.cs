using System;

namespace ClassService.Entities;

public class CachedUser
{
    public Guid Id { get; set; } // trùng khớp id bên user service
    public string UserCode { get; set; } = string.Empty; // mã học sinh / giáo viên (VD: HS001)
    public string FullName { get; set; } = string.Empty; // họ tên đầy đủ
    public string Role { get; set; } = string.Empty; // Vai trò (Student hoặc Teacher)
    public string? StudentStatus { get; set; } // Trạng thái học sinh (Active, Graduated, v.v.)
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow; // thời gian cập nhật lần cuối
}

// Sử dụng cho việc lưu trữ tạm thời dữ liệu người dùng trong bộ nhớ cache hoặc database nội bộ
// để các service khác có thể truy vấn nhanh.
// Lưu ý: Cần cơ chế đồng bộ hóa (synchronization) khi dữ liệu gốc thay đổi ở service nguồn.
