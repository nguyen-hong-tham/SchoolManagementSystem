using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassService.DTOs.TeachingAssignments;
using ClassService.Data;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using ClassService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Services;

public class TeachingAssignmentService : ITeachingAssignmentService
{
    private readonly ITeachingAssignmentRepository _teachingAssignmentRepository;
    private readonly IClassRepository _classRepository;

    private readonly ApplicationDbContext _dbContext;

    public TeachingAssignmentService(
        ITeachingAssignmentRepository teachingAssignmentRepository,
        IClassRepository classRepository,
        ApplicationDbContext dbContext
    )
    {
        _teachingAssignmentRepository = teachingAssignmentRepository;
        _classRepository = classRepository;
        _dbContext = dbContext;
    }

    public async Task<TeachingAssignmentResponseDto> AssignTeacherAsync(
        Guid classId,
        AssignTeacherDto dto
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
            throw new InvalidOperationException($"Không thể phân công giảng dạy: {cachedTeacher.FullName} không phải là giáo viên.");
        }

        var currentAssignment = await _teachingAssignmentRepository.GetAssignmentAsync(
            classId,
            dto.SubjectId,
            dto.SchoolYear
        );
        if (currentAssignment != null)
        {
            throw new InvalidOperationException(
                "Môn học này đã được phân công giáo viên bộ môn trong lớp này."
            );
        }

        var entity = new TeachingAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = dto.TeacherId,
            SubjectId = dto.SubjectId,
            ClassId = classId,
            SchoolYear = dto.SchoolYear,
            AssignedDate = DateTime.UtcNow,
        };

        await _teachingAssignmentRepository.AddAsync(entity);
        await _teachingAssignmentRepository.SaveChangesAsync();

        return MapToResponseDto(entity);
    }

    public async Task<TeachingAssignmentResponseDto> ChangeTeacherAsync(
        Guid classId,
        Guid subjectId,
        AssignTeacherDto dto
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
            throw new InvalidOperationException($"Không thể phân công giảng dạy: {cachedTeacher.FullName} không phải là giáo viên.");
        }

        var currentAssignment = await _teachingAssignmentRepository.GetAssignmentAsync(
            classId,
            subjectId,
            dto.SchoolYear
        );
        if (currentAssignment == null)
        {
            throw new InvalidOperationException(
                "Môn học này chưa được phân công giáo viên bộ môn trong lớp này để thay đổi."
            );
        }

        currentAssignment.TeacherId = dto.TeacherId;
        currentAssignment.AssignedDate = DateTime.UtcNow;

        _teachingAssignmentRepository.Update(currentAssignment);
        await _teachingAssignmentRepository.SaveChangesAsync();

        return MapToResponseDto(currentAssignment);
    }

    public async Task<IEnumerable<TeachingAssignmentResponseDto>> GetClassTeachersAsync(
        Guid classId,
        string schoolYear
    )
    {
        var targetClass = await _classRepository.GetByIdAsync(classId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {classId}");
        }

        var assignments = await _teachingAssignmentRepository.GetAssignmentsByClassAsync(
            classId,
            schoolYear
        );
        return assignments.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<TeachingAssignmentResponseDto>> GetTeacherClassesAsync(
        Guid teacherId,
        string schoolYear
    )
    {
        var assignments = await _teachingAssignmentRepository.GetAssignmentsByTeacherAsync(
            teacherId,
            schoolYear
        );
        return assignments.Select(MapToResponseDto);
    }

    private static TeachingAssignmentResponseDto MapToResponseDto(TeachingAssignment entity)
    {
        return new TeachingAssignmentResponseDto
        {
            Id = entity.Id,
            TeacherId = entity.TeacherId,
            SubjectId = entity.SubjectId,
            ClassId = entity.ClassId,
            SchoolYear = entity.SchoolYear,
            AssignedDate = entity.AssignedDate,
        };
    }
}
