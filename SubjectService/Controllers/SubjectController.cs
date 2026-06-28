using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubjectService.DTOs;
using SubjectService.Services;

namespace SubjectService.Controllers;

[ApiController]
[Route("api/subjects")]
[Authorize] // Bắt buộc đăng nhập với mọi endpoint theo mặc định
public class SubjectController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet] // Lấy danh sách môn học (Student, Teacher, Admin đều được gọi)
    public async Task<ActionResult<IEnumerable<SubjectResponse>>> GetAll(
        [FromQuery] int? gradeLevel
    )
    {
        var response = await _subjectService.GetAllSubjectsAsync(gradeLevel);
        return Ok(response);
    }

    // Lấy chi tiết môn học theo id
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubjectResponse>> GetSubject(Guid id)
    {
        try
        {
            var response = await _subjectService.GetSubjectByIdAsync(id);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // Tạo môn học mới (Chỉ Admin mới có quyền)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubjectResponse>> CreateSubject(
        [FromBody] CreateSubjectRequest request
    )
    {
        try
        {
            var response = await _subjectService.CreateSubjectAsync(request);
            return CreatedAtAction(nameof(GetSubject), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Cập nhật thông tin môn học (Chỉ Admin mới có quyền)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubjectRequest request)
    {
        try
        {
            var response = await _subjectService.UpdateSubjectAsync(id, request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // Xóa môn học (Chỉ Admin mới có quyền)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _subjectService.DeleteSubjectAsync(id);
            return Ok(new { message = "Xóa môn học thành công" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
