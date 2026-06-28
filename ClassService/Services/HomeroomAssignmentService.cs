using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassService.Data;
using ClassService.DTOs.HomeroomAssignments;
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
        var cachedTeacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u =>
            u.Id == dto.TeacherId
        );
        if (cachedTeacher == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy giáo viên với ID: {dto.TeacherId}");
        }
        if (cachedTeacher.Role != "Teacher")
        {
            throw new InvalidOperationException(
                $"Không thể phân công chủ nhiệm: {cachedTeacher.FullName} không phải là giáo viên."
            );
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

        return new HomeroomAssignmentResponseDto
        {
            Id = newAssignment.Id,
            ClassId = newAssignment.ClassId,
            TeacherId = newAssignment.TeacherId,
            SchoolYear = newAssignment.SchoolYear,
            AssignedDate = newAssignment.AssignedDate,
            TeacherName = cachedTeacher.FullName,
            TeacherCode = cachedTeacher.UserCode,
        };
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
        var cachedTeacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u =>
            u.Id == dto.TeacherId
        );
        if (cachedTeacher == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy giáo viên với ID: {dto.TeacherId}");
        }
        if (cachedTeacher.Role != "Teacher")
        {
            throw new InvalidOperationException(
                $"Không thể phân công chủ nhiệm: {cachedTeacher.FullName} không phải là giáo viên."
            );
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

        return new HomeroomAssignmentResponseDto
        {
            Id = currentHomeroom.Id,
            ClassId = currentHomeroom.ClassId,
            TeacherId = currentHomeroom.TeacherId,
            SchoolYear = currentHomeroom.SchoolYear,
            AssignedDate = currentHomeroom.AssignedDate,
            TeacherName = cachedTeacher.FullName,
            TeacherCode = cachedTeacher.UserCode,
        };
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
        if (homeroom == null)
            return null;
        var teacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u =>
            u.Id == homeroom.TeacherId
        );
        return new HomeroomAssignmentResponseDto
        {
            Id = homeroom.Id,
            ClassId = homeroom.ClassId,
            TeacherId = homeroom.TeacherId,
            SchoolYear = homeroom.SchoolYear,
            AssignedDate = homeroom.AssignedDate,
            TeacherName = teacher?.FullName ?? string.Empty,
            TeacherCode = teacher?.UserCode ?? string.Empty,
        };
    }

    // Lấy danh sách GVCN theo năm học
    public async Task<IEnumerable<HomeroomAssignmentResponseDto>> GetTeacherHomeroomHistoryAsync(
        Guid teacherId
    )
    {
        var history = await _homeroomRepository.GetHistoryByTeacherAsync(teacherId);
        var teacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == teacherId);
        return history
            .Select(h => new HomeroomAssignmentResponseDto
            {
                Id = h.Id,
                ClassId = h.ClassId,
                TeacherId = h.TeacherId,
                SchoolYear = h.SchoolYear,
                AssignedDate = h.AssignedDate,
                TeacherName = teacher?.FullName ?? string.Empty,
                TeacherCode = teacher?.UserCode ?? string.Empty,
            })
            .ToList();
    }

    // Lấy tất cả phân công GVCN theo năm học
    public async Task<IEnumerable<HomeroomAssignmentResponseDto>> GetAllHomeroomsAsync(
        string? schoolYear
    )
    {
        List<HomeroomAssignment> homerooms;
        if (string.IsNullOrEmpty(schoolYear))
        {
            homerooms = await _dbContext.HomeroomAssignments.ToListAsync();
        }
        else
        {
            homerooms = await _dbContext.HomeroomAssignments
                .Where(h => h.SchoolYear == schoolYear)
                .ToListAsync();
        }

        var teacherIds = homerooms.Select(h => h.TeacherId).Distinct().ToList();
        var teachers = await _dbContext
            .CachedUsers.Where(u => teacherIds.Contains(u.Id))
            .ToListAsync();

        return homerooms
            .Select(h =>
            {
                var teacher = teachers.FirstOrDefault(t => t.Id == h.TeacherId);
                return new HomeroomAssignmentResponseDto
                {
                    Id = h.Id,
                    ClassId = h.ClassId,
                    TeacherId = h.TeacherId,
                    SchoolYear = h.SchoolYear,
                    AssignedDate = h.AssignedDate,
                    TeacherName = teacher?.FullName ?? string.Empty,
                    TeacherCode = teacher?.UserCode ?? string.Empty,
                };
            })
            .ToList();
    }
}
