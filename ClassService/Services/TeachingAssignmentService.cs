using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassService.Data;
using ClassService.DTOs.TeachingAssignments;
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
                $"Không thể phân công giảng dạy: {cachedTeacher.FullName} không phải là giáo viên."
            );
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

        var cachedSubject = await _dbContext.CachedSubjects.FirstOrDefaultAsync(s =>
            s.Id == dto.SubjectId
        );
        return new TeachingAssignmentResponseDto
        {
            Id = entity.Id,
            TeacherId = entity.TeacherId,
            SubjectId = entity.SubjectId,
            ClassId = entity.ClassId,
            SchoolYear = entity.SchoolYear,
            AssignedDate = entity.AssignedDate,
            TeacherName = cachedTeacher.FullName,
            TeacherCode = cachedTeacher.UserCode,
            SubjectName = cachedSubject?.Name ?? string.Empty,
        };
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
                $"Không thể phân công giảng dạy: {cachedTeacher.FullName} không phải là giáo viên."
            );
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

        var cachedSubject = await _dbContext.CachedSubjects.FirstOrDefaultAsync(s =>
            s.Id == subjectId
        );
        return new TeachingAssignmentResponseDto
        {
            Id = currentAssignment.Id,
            TeacherId = currentAssignment.TeacherId,
            SubjectId = currentAssignment.SubjectId,
            ClassId = currentAssignment.ClassId,
            SchoolYear = currentAssignment.SchoolYear,
            AssignedDate = currentAssignment.AssignedDate,
            TeacherName = cachedTeacher.FullName,
            TeacherCode = cachedTeacher.UserCode,
            SubjectName = cachedSubject?.Name ?? string.Empty,
        };
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

        var teacherIds = assignments.Select(a => a.TeacherId).Distinct().ToList();
        var teachers = await _dbContext
            .CachedUsers.Where(u => teacherIds.Contains(u.Id))
            .ToListAsync();

        var subjectIds = assignments.Select(a => a.SubjectId).Distinct().ToList();
        var subjects = await _dbContext
            .CachedSubjects.Where(s => subjectIds.Contains(s.Id))
            .ToListAsync();

        return assignments
            .Select(entity =>
            {
                var teacher = teachers.FirstOrDefault(t => t.Id == entity.TeacherId);
                var subject = subjects.FirstOrDefault(s => s.Id == entity.SubjectId);
                return new TeachingAssignmentResponseDto
                {
                    Id = entity.Id,
                    TeacherId = entity.TeacherId,
                    SubjectId = entity.SubjectId,
                    ClassId = entity.ClassId,
                    SchoolYear = entity.SchoolYear,
                    AssignedDate = entity.AssignedDate,
                    TeacherName = teacher?.FullName ?? string.Empty,
                    TeacherCode = teacher?.UserCode ?? string.Empty,
                    SubjectName = subject?.Name ?? string.Empty,
                };
            })
            .ToList();
    }

    public async Task<IEnumerable<TeachingAssignmentResponseDto>> GetTeacherClassesAsync(
        Guid teacherId,
        string? schoolYear
    )
    {
        List<TeachingAssignment> assignments;
        if (string.IsNullOrEmpty(schoolYear))
        {
            assignments = await _dbContext.TeachingAssignments
                .Where(x => x.TeacherId == teacherId)
                .ToListAsync();
        }
        else
        {
            assignments = await _teachingAssignmentRepository.GetAssignmentsByTeacherAsync(
                teacherId,
                schoolYear
            );
        }

        var teacher = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == teacherId);

        var subjectIds = assignments.Select(a => a.SubjectId).Distinct().ToList();
        var subjects = await _dbContext
            .CachedSubjects.Where(s => subjectIds.Contains(s.Id))
            .ToListAsync();

        return assignments
            .Select(entity =>
            {
                var subject = subjects.FirstOrDefault(s => s.Id == entity.SubjectId);
                return new TeachingAssignmentResponseDto
                {
                    Id = entity.Id,
                    TeacherId = entity.TeacherId,
                    SubjectId = entity.SubjectId,
                    ClassId = entity.ClassId,
                    SchoolYear = entity.SchoolYear,
                    AssignedDate = entity.AssignedDate,
                    TeacherName = teacher?.FullName ?? string.Empty,
                    TeacherCode = teacher?.UserCode ?? string.Empty,
                    SubjectName = subject?.Name ?? string.Empty,
                };
            })
            .ToList();
    }

    public async Task<IEnumerable<TeachingAssignmentResponseDto>> GetAllAssignmentsAsync(
        string? schoolYear
    )
    {
        List<TeachingAssignment> assignments;
        if (string.IsNullOrEmpty(schoolYear))
        {
            assignments = await _dbContext.TeachingAssignments.ToListAsync();
        }
        else
        {
            assignments = await _dbContext.TeachingAssignments
                .Where(x => x.SchoolYear == schoolYear)
                .ToListAsync();
        }

        var teacherIds = assignments.Select(a => a.TeacherId).Distinct().ToList();
        var teachers = await _dbContext
            .CachedUsers.Where(u => teacherIds.Contains(u.Id))
            .ToListAsync();

        var subjectIds = assignments.Select(a => a.SubjectId).Distinct().ToList();
        var subjects = await _dbContext
            .CachedSubjects.Where(s => subjectIds.Contains(s.Id))
            .ToListAsync();

        var classes = await _classRepository.GetAllAsync();

        return assignments
            .Select(entity =>
            {
                var teacher = teachers.FirstOrDefault(t => t.Id == entity.TeacherId);
                var subject = subjects.FirstOrDefault(s => s.Id == entity.SubjectId);
                return new TeachingAssignmentResponseDto
                {
                    Id = entity.Id,
                    TeacherId = entity.TeacherId,
                    SubjectId = entity.SubjectId,
                    ClassId = entity.ClassId,
                    SchoolYear = entity.SchoolYear,
                    AssignedDate = entity.AssignedDate,
                    TeacherName = teacher?.FullName ?? string.Empty,
                    TeacherCode = teacher?.UserCode ?? string.Empty,
                    SubjectName = subject?.Name ?? string.Empty,
                };
            })
            .ToList();
    }

    public async Task<bool> RemoveAssignmentAsync(Guid classId, Guid subjectId)
    {
        var targetClass = await _classRepository.GetByIdAsync(classId);
        if (targetClass == null) return false;

        var currentAssignment = await _teachingAssignmentRepository.GetAssignmentAsync(
            classId,
            subjectId,
            targetClass.SchoolYear
        );
        if (currentAssignment == null) return false;

        _teachingAssignmentRepository.Delete(currentAssignment);
        await _teachingAssignmentRepository.SaveChangesAsync();
        return true;
    }
}
