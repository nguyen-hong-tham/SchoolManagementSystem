using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassService.DTOs.TeachingAssignments;

namespace ClassService.Services.Interfaces;

public interface ITeachingAssignmentService
{
    Task<TeachingAssignmentResponseDto> AssignTeacherAsync(Guid classId, AssignTeacherDto dto);
    Task<TeachingAssignmentResponseDto> ChangeTeacherAsync(
        Guid classId,
        Guid subjectId,
        AssignTeacherDto dto
    );
    Task<IEnumerable<TeachingAssignmentResponseDto>> GetClassTeachersAsync(
        Guid classId,
        string schoolYear
    );
    Task<IEnumerable<TeachingAssignmentResponseDto>> GetTeacherClassesAsync(
        Guid teacherId,
        string schoolYear
    );
}
