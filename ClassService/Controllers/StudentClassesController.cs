using ClassService.DTOs.StudentClasses;
using ClassService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClassService.Controllers;

[ApiController]
[Route("api/student-classes")]
public class StudentClassesController : ControllerBase
{
    private readonly IStudentClassService _studentClassService;

    public StudentClassesController(IStudentClassService studentClassService)
    {
        _studentClassService = studentClassService;
    }

    // 1 post: thêm học sinh vào lớp
    [HttpPost("classes/{classId:guid}/students")]
    public async Task<ActionResult<StudentClassResponseDto>> AssignStudent(
        Guid classId,
        [FromBody] AssignStudentDto dto
    )
    {
        try
        {
            var result = await _studentClassService.AssignStudentAsync(classId, dto);
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

    // 2 get : danh sách học sinh đang học trong lớp A
    [HttpGet("classes/{classId:guid}/students")]
    public async Task<ActionResult<IEnumerable<StudentClassResponseDto>>> GetStudents(Guid classId, [FromQuery] bool onlyCurrent = false)
    {
        try
        {
            var result = await _studentClassService.GetStudentsByClassIdAsync(classId, onlyCurrent);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // 3 delete: xóa học sinh khỏi lớp
    [HttpDelete("classes/{classId:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> RemoveStudent(Guid classId, Guid studentId)
    {
        var success = await _studentClassService.RemoveStudentAsync(classId, studentId);
        if (!success)
        {
            return BadRequest(
                new { message = "Học sinh không thuộc lớp này hoặc không phải lớp học hiện tại." }
            );
        }
        return NoContent();
    }

    // 4 post : chuyển lớp học sinh
    [HttpPost("students/{studentId:guid}/transfer")]
    public async Task<ActionResult<StudentClassResponseDto>> TransferStudent(
        Guid studentId,
        [FromBody] TransferStudentDto dto
    )
    {
        try
        {
            var result = await _studentClassService.TransferStudentAsync(studentId, dto);
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

    // 5 post: lên lớp học sinh
    [HttpPost("students/{studentId:guid}/promote")]
    public async Task<ActionResult<StudentClassResponseDto>> PromoteStudent(
        Guid studentId,
        [FromBody] PromoteStudentDto dto
    )
    {
        try
        {
            var result = await _studentClassService.PromoteStudentAsync(studentId, dto);
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

    [HttpGet("students/{studentId:guid}/history")]
    public async Task<ActionResult<IEnumerable<StudentClassResponseDto>>> GetStudentHistory(Guid studentId)
    {
        var result = await _studentClassService.GetClassHistoryAsync(studentId);
        return Ok(result);
    }

    [HttpGet("assigned-student-ids")]
    public async Task<ActionResult<IEnumerable<Guid>>> GetAssignedStudentIds()
    {
        var result = await _studentClassService.GetAssignedStudentIdsAsync();
        return Ok(result);
    }

    [HttpPost("promote-batch")]
    public async Task<ActionResult<IEnumerable<StudentClassResponseDto>>> PromoteBatch([FromBody] PromoteBatchDto dto)
    {
        try
        {
            var result = await _studentClassService.PromoteBatchAsync(dto);
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
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
