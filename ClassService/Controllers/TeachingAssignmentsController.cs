using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassService.DTOs.TeachingAssignments;
using ClassService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClassService.Controllers;

[ApiController]
[Route("api")]
public class TeachingAssignmentsController : ControllerBase
{
    private readonly ITeachingAssignmentService _teachingService;

    public TeachingAssignmentsController(ITeachingAssignmentService teachingService)
    {
        _teachingService = teachingService;
    }

    [HttpPost("classes/{classId:guid}/teachers")]
    public async Task<ActionResult<TeachingAssignmentResponseDto>> AssignTeacher(
        Guid classId,
        [FromBody] AssignTeacherDto dto
    )
    {
        try
        {
            var result = await _teachingService.AssignTeacherAsync(classId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("classes/{classId:guid}/teachers/{subjectId:guid}")]
    public async Task<ActionResult<TeachingAssignmentResponseDto>> ChangeTeacher(
        Guid classId,
        Guid subjectId,
        [FromBody] AssignTeacherDto dto
    )
    {
        try
        {
            var result = await _teachingService.ChangeTeacherAsync(classId, subjectId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("classes/{classId:guid}/teachers")]
    public async Task<ActionResult<IEnumerable<TeachingAssignmentResponseDto>>> GetClassTeachers(
        Guid classId,
        [FromQuery] string schoolYear
    )
    {
        try
        {
            var result = await _teachingService.GetClassTeachersAsync(classId, schoolYear);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("teachers/{teacherId:guid}/classes")]
    public async Task<ActionResult<IEnumerable<TeachingAssignmentResponseDto>>> GetTeacherClasses(
        Guid teacherId,
        [FromQuery] string? schoolYear
    )
    {
        var result = await _teachingService.GetTeacherClassesAsync(teacherId, schoolYear);
        return Ok(result);
    }

    [HttpGet("teaching-assignments")]
    public async Task<ActionResult<IEnumerable<TeachingAssignmentResponseDto>>> GetAllAssignments([FromQuery] string? schoolYear)
    {
        var result = await _teachingService.GetAllAssignmentsAsync(schoolYear);
        return Ok(result);
    }

    [HttpDelete("classes/{classId:guid}/teachers/{subjectId:guid}")]
    public async Task<IActionResult> RemoveTeacherAssignment(Guid classId, Guid subjectId)
    {
        var success = await _teachingService.RemoveAssignmentAsync(classId, subjectId);
        if (!success)
        {
            return BadRequest(new { message = "Không thể xóa phân công giảng dạy hoặc phân công không tồn tại." });
        }
        return NoContent();
    }
}
