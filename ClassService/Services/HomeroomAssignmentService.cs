using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassService.DTOs.HomeroomAssignments;
using ClassService.Data;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using ClassService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Services;

public class HomeroomAssignmentService : IHomeroomAssignmentService
{
    private readonly IHomeroomAssignmentRepository _homeroomRepository;
    private readonly IClassRepository _classRepository;

    private readonly ApplicationDbContext _dbContext;

    public HomeroomAssignmentService(
        IHomeroomAssignmentRepository homeroomRepository,
        IClassRepository classRepository,
        ApplicationDbContext dbContext
    )
    {
        _homeroomRepository = homeroomRepository;
        _classRepository = classRepository;
        _dbContext = dbContext;
    }

    // Thêm giáo viên chủ nhiệm
    public async Task<HomeroomAssignmentResponseDto> AssignHomeroomAsync(
        Guid classId,
        AssignHomeroomDto dto
    )
    {
        var targetClass = await _classRepository.GetByIdAsync(classId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {classId}");
        }

        // Kiểm tra giáo viên từ bảng đệm CachedUsers
        var cachedTeacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == dto.TeacherId);
        if (cachedTeacher == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy giáo viên với ID: {dto.TeacherId}");
        }
        if (cachedTeacher.Role != "Teacher")
        {
            throw new InvalidOperationException($"Không thể phân công chủ nhiệm: {cachedTeacher.FullName} không phải là giáo viên.");
        }

        var currentHomeroom = await _homeroomRepository.GetActiveAssignmentByClassAsync(
            classId,
            dto.SchoolYear
        );
        if (currentHomeroom != null)
        {
            throw new InvalidOperationException("Lớp đã có giáo viên chủ nhiệm trong năm học này.");
        }

        var newAssignment = new HomeroomAssignment
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            TeacherId = dto.TeacherId,
            SchoolYear = dto.SchoolYear,
            AssignedDate = DateTime.UtcNow,
        };

        await _homeroomRepository.AddAsync(newAssignment);
        await _homeroomRepository.SaveChangesAsync();

        return MapToResponseDto(newAssignment);
    }

    // Đổi giáo viên chủ nhiệm
    public async Task<HomeroomAssignmentResponseDto> ChangeHomeroomAsync(
        Guid classId,
        AssignHomeroomDto dto
    )
    {
        var targetClass = await _classRepository.GetByIdAsync(classId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {classId}");
        }

        // Kiểm tra giáo viên từ bảng đệm CachedUsers
        var cachedTeacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == dto.TeacherId);
        if (cachedTeacher == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy giáo viên với ID: {dto.TeacherId}");
        }
        if (cachedTeacher.Role != "Teacher")
        {
            throw new InvalidOperationException($"Không thể phân công chủ nhiệm: {cachedTeacher.FullName} không phải là giáo viên.");
        }

        var currentHomeroom = await _homeroomRepository.GetActiveAssignmentByClassAsync(
            classId,
            dto.SchoolYear
        );
        if (currentHomeroom == null)
        {
            throw new InvalidOperationException(
                "Lớp chưa có giáo viên chủ nhiệm trong năm học này để thay đổi."
            );
        }

        currentHomeroom.TeacherId = dto.TeacherId;
        currentHomeroom.AssignedDate = DateTime.UtcNow;

        _homeroomRepository.Update(currentHomeroom);
        await _homeroomRepository.SaveChangesAsync();

        return MapToResponseDto(currentHomeroom);
    }

    // Lấy thông tin 1 GVCN
    public async Task<HomeroomAssignmentResponseDto?> GetCurrentHomeroomAsync(
        Guid classId,
        string schoolYear
    )
    {
        var homeroom = await _homeroomRepository.GetActiveAssignmentByClassAsync(
            classId,
            schoolYear
        );
        return homeroom != null ? MapToResponseDto(homeroom) : null;
    }

    // Lấy danh sách GVCN theo năm học
    public async Task<IEnumerable<HomeroomAssignmentResponseDto>> GetTeacherHomeroomHistoryAsync(
        Guid teacherId
    )
    {
        var history = await _homeroomRepository.GetHistoryByTeacherAsync(teacherId);
        return history.Select(MapToResponseDto);
    }

    private static HomeroomAssignmentResponseDto MapToResponseDto(HomeroomAssignment entity)
    {
        return new HomeroomAssignmentResponseDto
        {
            Id = entity.Id,
            ClassId = entity.ClassId,
            TeacherId = entity.TeacherId,
            SchoolYear = entity.SchoolYear,
            AssignedDate = entity.AssignedDate,
        };
    }
}
