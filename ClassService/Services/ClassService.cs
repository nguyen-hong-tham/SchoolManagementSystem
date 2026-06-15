using ClassService.DTOs.Classes;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using ClassService.Services.Interfaces;

namespace ClassService.Services;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;

    public ClassService(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<IEnumerable<ClassResponseDto>> GetAllAsync()
    {
        var classes = await _classRepository.GetAllAsync();
        return classes.Select(MapToResponseDto);
    }

    public async Task<ClassResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _classRepository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {id}");
        }
        return MapToResponseDto(entity);
    }

    public async Task<ClassResponseDto> CreateAsync(CreateClassDto dto)
    {
        var entity = new Class
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            GradeLevel = dto.GradeLevel,
            SchoolYear = dto.SchoolYear,
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
            CreatedAt = entity.CreatedAt
        };
    }
}