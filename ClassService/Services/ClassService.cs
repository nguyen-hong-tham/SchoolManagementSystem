using ClassService.Data;
using ClassService.DTOs.Classes;
using ClassService.Entities;
using ClassService.Helpers;
using ClassService.Repositories.Interfaces;
using ClassService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Services;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly ApplicationDbContext _dbContext;

    public ClassService(IClassRepository classRepository, ApplicationDbContext dbContext)
    {
        _classRepository = classRepository;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ClassResponseDto>> GetAllAsync()
    {
        var classes = await _classRepository.GetAllAsync();
        var classList = classes.ToList();
        var classIds = classList.Select(c => c.Id).ToList();

        // Get homeroom assignments
        var homerooms = await _dbContext.HomeroomAssignments
            .Where(h => classIds.Contains(h.ClassId))
            .ToListAsync();
        var teacherIds = homerooms.Select(h => h.TeacherId).Distinct().ToList();
        var teachers = await _dbContext.CachedUsers
            .Where(u => teacherIds.Contains(u.Id))
            .ToListAsync();

        // Get current student counts per class
        var studentCounts = await _dbContext.StudentClasses
            .Where(sc => classIds.Contains(sc.ClassId) && sc.IsCurrent)
            .GroupBy(sc => sc.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ClassId, g => g.Count);

        return classList.Select(c =>
        {
            var hr = homerooms.FirstOrDefault(h => h.ClassId == c.Id && h.SchoolYear == c.SchoolYear)
                     ?? homerooms.FirstOrDefault(h => h.ClassId == c.Id);
            var teacher = hr != null ? teachers.FirstOrDefault(t => t.Id == hr.TeacherId) : null;
            int count = studentCounts.TryGetValue(c.Id, out var cnt) ? cnt : 0;

            return new ClassResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                GradeLevel = c.GradeLevel,
                SchoolYear = c.SchoolYear,
                Capacity = c.Capacity > 0 ? c.Capacity : 45,
                CurrentStudentCount = count,
                HomeroomTeacherId = teacher?.Id,
                HomeroomTeacherName = teacher?.FullName,
                HomeroomTeacherCode = teacher?.UserCode,
                CreatedAt = c.CreatedAt
            };
        });
    }

    public async Task<ClassResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _classRepository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {id}");
        }

        var hr = await _dbContext.HomeroomAssignments
            .FirstOrDefaultAsync(h => h.ClassId == id && h.SchoolYear == entity.SchoolYear)
            ?? await _dbContext.HomeroomAssignments
                .Where(h => h.ClassId == id)
                .OrderByDescending(h => h.AssignedDate)
                .FirstOrDefaultAsync();
        var teacher = hr != null
            ? await UserCacheHelper.GetOrFetchCachedUserAsync(_dbContext, hr.TeacherId)
            : null;
        var count = await _dbContext.StudentClasses
            .CountAsync(sc => sc.ClassId == id && sc.IsCurrent);

        return new ClassResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            GradeLevel = entity.GradeLevel,
            SchoolYear = entity.SchoolYear,
            Capacity = entity.Capacity > 0 ? entity.Capacity : 45,
            CurrentStudentCount = count,
            HomeroomTeacherId = teacher?.Id,
            HomeroomTeacherName = teacher?.FullName,
            HomeroomTeacherCode = teacher?.UserCode,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<ClassResponseDto> CreateAsync(CreateClassDto dto)
    {
        var existingClasses = await _classRepository.GetAllAsync();
        if (existingClasses.Any(c => string.Equals(c.Name, dto.Name, StringComparison.OrdinalIgnoreCase) && c.SchoolYear == dto.SchoolYear))
        {
            throw new InvalidOperationException($"Lớp học '{dto.Name}' đã tồn tại trong năm học {dto.SchoolYear}.");
        }

        var entity = new Class
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            GradeLevel = dto.GradeLevel,
            SchoolYear = dto.SchoolYear,
            Capacity = 45,
            CreatedAt = DateTime.UtcNow
        };

        await _classRepository.AddAsync(entity);
        await _classRepository.SaveChangesAsync();

        return MapToResponseDto(entity);
    }

    public async Task<ClassResponseDto?> UpdateAsync(Guid id, UpdateClassDto dto)
    {
        var entity = await _classRepository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {id}");
        }

        var existingClasses = await _classRepository.GetAllAsync();
        if (existingClasses.Any(c => c.Id != id && string.Equals(c.Name, dto.Name, StringComparison.OrdinalIgnoreCase) && c.SchoolYear == dto.SchoolYear))
        {
            throw new InvalidOperationException($"Lớp học '{dto.Name}' đã tồn tại trong năm học {dto.SchoolYear}.");
        }

        entity.Name = dto.Name;
        entity.GradeLevel = dto.GradeLevel;
        entity.SchoolYear = dto.SchoolYear;

        _classRepository.Update(entity);
        await _classRepository.SaveChangesAsync();

        return MapToResponseDto(entity);
    }

    public async Task<ClassResponseDto?> DeleteAsync(Guid id)
    {
        var entity = await _classRepository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {id}");
        }

        // 1. Kiểm tra học sinh đang thuộc lớp
        var studentCount = await _dbContext.StudentClasses
            .CountAsync(sc => sc.ClassId == id && sc.IsCurrent);
        if (studentCount > 0)
        {
            throw new InvalidOperationException($"Không thể xóa lớp {entity.Name} vì lớp đang có {studentCount} học sinh. Vui lòng chuyển hoặc xóa học sinh khỏi lớp trước khi xóa.");
        }

        // 2. Kiểm tra thời khóa biểu
        var scheduleCount = await _dbContext.Schedules
            .CountAsync(s => s.ClassId == id);
        if (scheduleCount > 0)
        {
            throw new InvalidOperationException($"Không thể xóa lớp {entity.Name} vì lớp đã có {scheduleCount} tiết trong thời khóa biểu. Vui lòng xóa thời khóa biểu trước.");
        }

        // 3. Kiểm tra phân công giảng dạy
        var teachingCount = await _dbContext.TeachingAssignments
            .CountAsync(ta => ta.ClassId == id);
        if (teachingCount > 0)
        {
            throw new InvalidOperationException($"Không thể xóa lớp {entity.Name} vì lớp đã được phân công giáo viên giảng dạy.");
        }

        // 4. Xóa phân công chủ nhiệm nếu có
        var homerooms = await _dbContext.HomeroomAssignments.Where(h => h.ClassId == id).ToListAsync();
        if (homerooms.Any())
        {
            _dbContext.HomeroomAssignments.RemoveRange(homerooms);
        }

        _classRepository.Delete(entity);
        await _classRepository.SaveChangesAsync();

        return MapToResponseDto(entity);
    }

    private static ClassResponseDto MapToResponseDto(Class entity)
    {
        return new ClassResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            GradeLevel = entity.GradeLevel,
            SchoolYear = entity.SchoolYear,
            Capacity = entity.Capacity > 0 ? entity.Capacity : 45,
            CreatedAt = entity.CreatedAt
        };
    }
}