using System.Net.Http;
using System.Net.Http.Json;
using ClassService.Data;
using ClassService.DTOs.StudentClasses;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using ClassService.Services.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace ClassService.Services;

public class StudentClassService : IStudentClassService
{
    private readonly IStudentClassRepository _studentClassRepository;
    private readonly IClassRepository _classRepository;
    private readonly ApplicationDbContext _dbContext; // tiêm DbContext để query bảng đệm CacheUsers
    private readonly IPublishEndpoint _publishEndpoint;

    public StudentClassService(
        IStudentClassRepository studentClassRepository,
        IClassRepository classRepository,
        ApplicationDbContext dbContext,
        IPublishEndpoint publishEndpoint
    )
    {
        _studentClassRepository = studentClassRepository;
        _classRepository = classRepository;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
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

        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(
                    new StudentPromotedEvent
                    {
                        StudentId = dto.StudentId,
                        NewClassId = classId,
                        IsGraduating = false,
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[RabbitMQ Publish Info] Failed to publish StudentPromotedEvent in background: {ex.Message}"
                );
            }
        });

        // HTTP Sync Fallback (useful if RabbitMQ is not running)
        await SyncToUserServiceHttpAsync(dto.StudentId, classId, "Active");

        return await MapToResponseDtoAsync(studentClass);
    }

    // Xem danh sách học sinh của lớp
    public async Task<IEnumerable<StudentClassResponseDto>> GetStudentsByClassIdAsync(
        Guid classId,
        bool onlyCurrent = false
    )
    {
        var targetClass = await _classRepository.GetByIdAsync(classId);
        if (targetClass == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học với ID: {classId}");
        }

        var studentClasses = await _studentClassRepository.GetStudentsByClassIdAsync(
            classId,
            onlyCurrent
        );
        var studentIds = studentClasses.Select(x => x.StudentId).ToList();
        var students = await _dbContext
            .CachedUsers.Where(u => studentIds.Contains(u.Id))
            .ToListAsync();

        return studentClasses
            .Select(sc =>
            {
                var student = students.FirstOrDefault(s => s.Id == sc.StudentId);
                return new StudentClassResponseDto
                {
                    Id = sc.Id,
                    StudentId = sc.StudentId,
                    ClassId = sc.ClassId,
                    SchoolYear = sc.SchoolYear,
                    AssignedDate = sc.AssignedDate,
                    IsCurrent = sc.IsCurrent,
                    StudentName = student?.FullName ?? string.Empty,
                    StudentCode = student?.UserCode ?? string.Empty,
                };
            })
            .ToList();
    }

    // Xem lớp hiện tại của học sinh
    public async Task<StudentClassResponseDto?> GetCurrentClassAsync(Guid studentId)
    {
        var currentAssignment = await _studentClassRepository.GetCurrentStudentClassAsync(
            studentId
        );
        return currentAssignment != null ? await MapToResponseDtoAsync(currentAssignment) : null;
    }

    // Xem lịch sử học tập lớp học của học sinh
    public async Task<IEnumerable<StudentClassResponseDto>> GetClassHistoryAsync(Guid studentId)
    {
        var history = await _dbContext
            .StudentClasses.Where(x => x.StudentId == studentId)
            .OrderBy(x => x.SchoolYear)
            .ToListAsync();

        var dtos = new List<StudentClassResponseDto>();
        foreach (var item in history)
        {
            dtos.Add(await MapToResponseDtoAsync(item));
        }
        return dtos;
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

        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(
                    new StudentPromotedEvent
                    {
                        StudentId = studentId,
                        NewClassId = dto.NewClassId,
                        IsGraduating = false,
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[RabbitMQ Publish Info] Failed to publish StudentPromotedEvent in background: {ex.Message}"
                );
            }
        });

        // HTTP Sync Fallback (useful if RabbitMQ is not running)
        await SyncToUserServiceHttpAsync(studentId, dto.NewClassId, "Active");

        return await MapToResponseDtoAsync(newAssignment);
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

        var (currentAssignment, isIdempotent) = await ValidatePromotionRulesAsync(
            studentId,
            targetClass,
            dto.SchoolYear
        );
        if (isIdempotent && currentAssignment != null)
        {
            return await MapToResponseDtoAsync(currentAssignment);
        }

        if (currentAssignment != null)
        {
            currentAssignment.IsCurrent = false;
            currentAssignment.LeftDate = DateTime.UtcNow;
            currentAssignment.PromotionStatus = "Promoted";
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

        // Publish event to notify UserService of the promotion
        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(
                    new StudentPromotedEvent
                    {
                        StudentId = studentId,
                        NewClassId = dto.NewClassId,
                        IsGraduating = false,
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[RabbitMQ Publish Info] Failed to publish StudentPromotedEvent in background: {ex.Message}"
                );
            }
        });

        // HTTP Sync Fallback (useful if RabbitMQ is not running)
        await SyncToUserServiceHttpAsync(studentId, dto.NewClassId, "Active");

        return await MapToResponseDtoAsync(newAssignment);
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

        _ = Task.Run(async () =>
        {
            try
            {
                await _publishEndpoint.Publish(
                    new StudentPromotedEvent
                    {
                        StudentId = studentId,
                        NewClassId = null,
                        IsGraduating = false,
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[RabbitMQ Publish Info] Failed to publish StudentPromotedEvent in background: {ex.Message}"
                );
            }
        });

        // HTTP Sync Fallback (useful if RabbitMQ is not running)
        await SyncToUserServiceHttpAsync(studentId, null, "Active");

        return true;
    }

    private async Task<StudentClassResponseDto> MapToResponseDtoAsync(StudentClass entity)
    {
        var student = await _dbContext.CachedUsers.FirstOrDefaultAsync(u =>
            u.Id == entity.StudentId
        );
        var cls = await _classRepository.GetByIdAsync(entity.ClassId);
        return new StudentClassResponseDto
        {
            Id = entity.Id,
            StudentId = entity.StudentId,
            ClassId = entity.ClassId,
            ClassName = cls?.Name ?? string.Empty,
            SchoolYear = entity.SchoolYear,
            AssignedDate = entity.AssignedDate,
            IsCurrent = entity.IsCurrent,
            LeftDate = entity.LeftDate,
            PromotionStatus = entity.PromotionStatus,
            StudentName = student?.FullName ?? string.Empty,
            StudentCode = student?.UserCode ?? string.Empty,
        };
    }

    public async Task<IEnumerable<Guid>> GetAssignedStudentIdsAsync()
    {
        return await _dbContext
            .StudentClasses.Where(sc => sc.IsCurrent)
            .Select(sc => sc.StudentId)
            .ToListAsync();
    }

    private async Task<(
        StudentClass? CurrentAssignment,
        bool IsIdempotent
    )> ValidatePromotionRulesAsync(Guid studentId, Class targetClass, string schoolYear)
    {
        var student = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == studentId);
        if (student == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy học sinh với ID: {studentId}");
        }
        if (student.StudentStatus == "Graduated")
        {
            throw new InvalidOperationException(
                $"Học sinh {student.FullName} đã tốt nghiệp, không thể phân lớp."
            );
        }
        if (student.Role != "Student")
        {
            throw new InvalidOperationException(
                $"Không thể lên lớp: {student.FullName} không phải là học sinh."
            );
        }

        var currentAssignment = await _studentClassRepository.GetCurrentStudentClassAsync(
            studentId
        );
        if (currentAssignment != null)
        {
            if (
                currentAssignment.ClassId == targetClass.Id
                && currentAssignment.SchoolYear == schoolYear
            )
            {
                return (currentAssignment, true);
            }

            if (currentAssignment.SchoolYear == schoolYear)
            {
                throw new InvalidOperationException(
                    $"Học sinh {student.FullName} đã được xếp vào lớp khác trong năm học {schoolYear}."
                );
            }

            var currentClass = await _classRepository.GetByIdAsync(currentAssignment.ClassId);
            if (currentClass != null)
            {
                bool isValidProgression =
                    targetClass.GradeLevel == currentClass.GradeLevel
                    || targetClass.GradeLevel == currentClass.GradeLevel + 1;
                if (!isValidProgression)
                {
                    throw new InvalidOperationException(
                        $"Lớp học đích {targetClass.Name} (Khối {targetClass.GradeLevel}) không đúng tiến trình khối so với lớp hiện tại {currentClass.Name} (Khối {currentClass.GradeLevel})."
                    );
                }
            }
        }

        var currentStudentsCount = await _dbContext.StudentClasses.CountAsync(sc =>
            sc.ClassId == targetClass.Id && sc.IsCurrent
        );
        if (currentStudentsCount >= targetClass.Capacity)
        {
            throw new InvalidOperationException(
                $"Lớp học đích {targetClass.Name} đã đạt giới hạn tối đa ({targetClass.Capacity} học sinh)."
            );
        }

        return (currentAssignment, false);
    }

    private async Task<StudentClass> ValidateGraduationRulesAsync(Guid studentId)
    {
        var student = await _dbContext.CachedUsers.FirstOrDefaultAsync(u => u.Id == studentId);
        if (student == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy học sinh với ID: {studentId}");
        }
        if (student.StudentStatus == "Graduated")
        {
            throw new InvalidOperationException(
                $"Học sinh {student.FullName} đã tốt nghiệp từ trước."
            );
        }

        var currentAssignment = await _studentClassRepository.GetCurrentStudentClassAsync(
            studentId
        );
        if (currentAssignment == null)
        {
            throw new InvalidOperationException(
                $"Học sinh {student.FullName} không có lớp học hiện tại để tốt nghiệp."
            );
        }

        var currentClass = await _classRepository.GetByIdAsync(currentAssignment.ClassId);
        if (currentClass == null || currentClass.GradeLevel != 12)
        {
            throw new InvalidOperationException(
                $"Chỉ học sinh Khối 12 mới được phép Tốt nghiệp. Học sinh {student.FullName} đang ở lớp Khối {(currentClass?.GradeLevel.ToString() ?? "không xác định")}."
            );
        }

        return currentAssignment;
    }

    public async Task<IEnumerable<StudentClassResponseDto>> PromoteBatchAsync(PromoteBatchDto dto)
    {
        var results = new List<StudentClassResponseDto>();
        var eventsToPublish = new List<StudentPromotedEvent>();

        Class? targetClass = null;
        if (!dto.IsGraduating)
        {
            if (!dto.NewClassId.HasValue)
            {
                throw new InvalidOperationException(
                    "Lớp học đích là bắt buộc khi học sinh lên lớp."
                );
            }
            targetClass = await _classRepository.GetByIdAsync(dto.NewClassId.Value);
            if (targetClass == null)
            {
                throw new KeyNotFoundException(
                    $"Không tìm thấy lớp học đích với ID: {dto.NewClassId.Value}"
                );
            }

            if (targetClass.SchoolYear != dto.SchoolYear)
            {
                throw new InvalidOperationException(
                    $"Năm học của lớp học đích ({targetClass.SchoolYear}) không khớp với năm học đích yêu cầu ({dto.SchoolYear})."
                );
            }
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (var studentId in dto.StudentIds)
            {
                if (dto.IsGraduating)
                {
                    var currentAssignment = await ValidateGraduationRulesAsync(studentId);

                    currentAssignment.IsCurrent = false;
                    currentAssignment.LeftDate = DateTime.UtcNow;
                    currentAssignment.PromotionStatus = "Graduated";

                    await _studentClassRepository.SaveChangesAsync();
                    results.Add(await MapToResponseDtoAsync(currentAssignment));

                    eventsToPublish.Add(
                        new StudentPromotedEvent
                        {
                            StudentId = studentId,
                            NewClassId = null,
                            IsGraduating = true,
                        }
                    );
                }
                else
                {
                    var (currentAssignment, isIdempotent) = await ValidatePromotionRulesAsync(
                        studentId,
                        targetClass!,
                        dto.SchoolYear
                    );
                    if (isIdempotent && currentAssignment != null)
                    {
                        results.Add(await MapToResponseDtoAsync(currentAssignment));
                        continue;
                    }

                    if (currentAssignment != null)
                    {
                        currentAssignment.IsCurrent = false;
                        currentAssignment.LeftDate = DateTime.UtcNow;
                        currentAssignment.PromotionStatus = "Promoted";
                    }

                    var newAssignment = new StudentClass
                    {
                        Id = Guid.NewGuid(),
                        StudentId = studentId,
                        ClassId = dto.NewClassId!.Value,
                        SchoolYear = dto.SchoolYear,
                        AssignedDate = DateTime.UtcNow,
                        IsCurrent = true,
                    };

                    await _studentClassRepository.AddAsync(newAssignment);
                    await _studentClassRepository.SaveChangesAsync();

                    results.Add(await MapToResponseDtoAsync(newAssignment));

                    eventsToPublish.Add(
                        new StudentPromotedEvent
                        {
                            StudentId = studentId,
                            NewClassId = dto.NewClassId!.Value,
                            IsGraduating = false,
                        }
                    );
                }
            }

            await transaction.CommitAsync();

            // Publish events after transaction successfully commits
            foreach (var ev in eventsToPublish)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _publishEndpoint.Publish(ev);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[RabbitMQ Publish Info] Failed to publish StudentPromotedEvent in background: {ex.Message}"
                        );
                    }
                });
                await SyncToUserServiceHttpAsync(
                    ev.StudentId,
                    ev.NewClassId,
                    ev.IsGraduating ? "Graduated" : "Active"
                );
            }

            return results;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task SyncToUserServiceHttpAsync(Guid studentId, Guid? newClassId, string? status)
    {
        try
        {
            using var client = new HttpClient();
            var payload = new
            {
                StudentId = studentId,
                NewClassId = newClassId,
                StudentStatus = status,
            };

            Console.WriteLine(
                $"[HTTP Sync] Transmitting sync request to UserService: StudentId={studentId}, NewClassId={newClassId}, Status={status}..."
            );

            var response = await client.PostAsJsonAsync(
                "http://localhost:5156/api/users/internal/sync-class",
                payload
            );
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"[HTTP Sync] Transmit SUCCESS: StudentId={studentId} successfully synced to UserService."
                );
            }
            else
            {
                Console.WriteLine(
                    $"[HTTP Sync Warning] Transmit FAILED: Failed to sync student {studentId} to UserService. Status: {response.StatusCode}"
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[HTTP Sync Warning] Transmit ERROR: Failed to sync student {studentId} to UserService: {ex.Message}"
            );
        }
    }
}
