using ClassService.Data;
using ClassService.DTOs.StudentClasses;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using ClassService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Services;

public class StudentClassService : IStudentClassService
{
    private readonly IStudentClassRepository _studentClassRepository;
    private readonly IClassRepository _classRepository;
    private readonly ApplicationDbContext _dbContext; // tiêm DbContext để query bảng đệm CacheUsers

    public StudentClassService(
        IStudentClassRepository studentClassRepository,
        IClassRepository classRepository,
        ApplicationDbContext dbContext
    )
    {
        _studentClassRepository = studentClassRepository;
        _classRepository = classRepository;
        _dbContext = dbContext;
    }

    // Thêm học sinh vào lớp
    public async Task<StudentClassResponseDto> AssignStudentAsync(
        Guid classId,
        AssignStudentDto dto
    )
    {
        var targetClass = await _classRepository.GetByIdAsync(classId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {classId}");
        }

        // truy vấn từ bảng đệm CachedUser thay vì gọi Api mạng sang userSerivce
        var cachedStudent = await _dbContext.CachedUsers.FirstOrDefaultAsync(u =>
            u.Id == dto.StudentId
        );
        if (cachedStudent == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy học sinh với ID: {dto.StudentId}");
        }

        if (cachedStudent.Role != "Student")
        {
            throw new InvalidOperationException(
                $"Không thể thêm: {cachedStudent.FullName} không phải là học sinh."
            );
        }
        // kiểm tra xem học sinh đã có lớp học hiện tại chưa
        var currentAssignment = await _studentClassRepository.GetCurrentStudentClassAsync(
            dto.StudentId
        );
        if (currentAssignment != null)
        {
            throw new InvalidOperationException("Học sinh đã có lớp học hiện tại.");
        }
        // lưu phân lớp
        var studentClass = new StudentClass
        {
            Id = Guid.NewGuid(),
            StudentId = dto.StudentId,
            ClassId = classId,
            SchoolYear = targetClass.SchoolYear,
            AssignedDate = DateTime.UtcNow,
            IsCurrent = true,
        };

        await _studentClassRepository.AddAsync(studentClass);
        await _studentClassRepository.SaveChangesAsync();

        return MapToResponseDto(studentClass);
    }

    // Xem danh sách học sinh của lớp
    public async Task<IEnumerable<StudentClassResponseDto>> GetStudentsByClassIdAsync(Guid classId)
    {
        var targetClass = await _classRepository.GetByIdAsync(classId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {classId}");
        }

        var studentClasses = await _studentClassRepository.GetStudentsByClassIdAsync(classId);
        return studentClasses.Select(MapToResponseDto);
    }

    // Xem lớp hiện tại của học sinh
    public async Task<StudentClassResponseDto?> GetCurrentClassAsync(Guid studentId)
    {
        var currentAssignment = await _studentClassRepository.GetCurrentStudentClassAsync(
            studentId
        );
        return currentAssignment != null ? MapToResponseDto(currentAssignment) : null;
    }

    // Chuyển lớp
    public async Task<StudentClassResponseDto> TransferStudentAsync(
        Guid studentId,
        TransferStudentDto dto
    )
    {
        var targetClass = await _classRepository.GetByIdAsync(dto.NewClassId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {dto.NewClassId}");
        }

        var currentAssignment = await _studentClassRepository.GetCurrentStudentClassAsync(
            studentId
        );
        if (currentAssignment == null)
        {
            throw new InvalidOperationException("Học sinh chưa có lớp học hiện tại để chuyển.");
        }

        if (currentAssignment.ClassId == dto.NewClassId)
        {
            throw new InvalidOperationException("Học sinh đã ở trong lớp học này.");
        }

        currentAssignment.IsCurrent = false;

        var newAssignment = new StudentClass
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            ClassId = dto.NewClassId,
            SchoolYear = targetClass.SchoolYear,
            AssignedDate = DateTime.UtcNow,
            IsCurrent = true,
        };

        await _studentClassRepository.AddAsync(newAssignment);
        await _studentClassRepository.SaveChangesAsync();

        return MapToResponseDto(newAssignment);
    }

    // Lên lớp
    public async Task<StudentClassResponseDto> PromoteStudentAsync(
        Guid studentId,
        PromoteStudentDto dto
    )
    {
        var targetClass = await _classRepository.GetByIdAsync(dto.NewClassId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {dto.NewClassId}");
        }

        var currentAssignment = await _studentClassRepository.GetCurrentStudentClassAsync(
            studentId
        );
        if (currentAssignment != null)
        {
            currentAssignment.IsCurrent = false;
        }

        var newAssignment = new StudentClass
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            ClassId = dto.NewClassId,
            SchoolYear = dto.SchoolYear,
            AssignedDate = DateTime.UtcNow,
            IsCurrent = true,
        };

        await _studentClassRepository.AddAsync(newAssignment);
        await _studentClassRepository.SaveChangesAsync();

        return MapToResponseDto(newAssignment);
    }

    // Xóa học sinh khỏi lớp
    public async Task<bool> RemoveStudentAsync(Guid classId, Guid studentId)
    {
        var currentAssignment = await _studentClassRepository.GetCurrentStudentClassAsync(
            studentId
        );
        if (currentAssignment == null || currentAssignment.ClassId != classId)
        {
            return false;
        }

        currentAssignment.IsCurrent = false;
        await _studentClassRepository.SaveChangesAsync();
        return true;
    }

    private static StudentClassResponseDto MapToResponseDto(StudentClass entity)
    {
        return new StudentClassResponseDto
        {
            Id = entity.Id,
            StudentId = entity.StudentId,
            ClassId = entity.ClassId,
            SchoolYear = entity.SchoolYear,
            AssignedDate = entity.AssignedDate,
            IsCurrent = entity.IsCurrent,
        };
    }
}
