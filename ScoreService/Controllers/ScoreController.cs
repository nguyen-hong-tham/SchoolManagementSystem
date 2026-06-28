using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreService.DtOs;
using ScoreService.Services;

namespace ScoreService.Controllers;

[ApiController]
[Route("api/scores")]
[Authorize]
public class ScoreController : ControllerBase
{
    private readonly IScoreService _scoreService;

    public ScoreController(IScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    // Tra cứu bảng điểm của học sinh (Học sinh chỉ xem được điểm của chính mình, Admin/Teacher được xem bất kỳ)
    [HttpGet("student/{studentId:guid}")]
    public async Task<ActionResult<IEnumerable<ScoreResponse>>> GetStudentScores(Guid studentId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

        if (currentUserRole == "Student" && currentUserId != studentId.ToString())
        {
            return Forbid("Bạn không có quyền truy cập bảng điểm của học sinh khác.");
        }

        var response = await _scoreService.GetStudentScoresAsync(studentId);
        return Ok(response);
    }

    // Nhập điểm mới (Chỉ Admin và Giáo viên được quyền nhập)
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<ScoreResponse>> CreateScore(
        [FromBody] CreateScoreRequest request
    )
    {
        // Lấy ID giáo viên nhập điểm trực tiếp từ Claim Token JWT
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var response = await _scoreService.CreateScoreAsync(request, currentUserId);
            return CreatedAtAction(
                nameof(GetStudentScores),
                new { studentId = response.StudentId },
                response
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Sửa điểm số (Chỉ Admin và Giáo viên được quyền)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateScore(Guid id, [FromBody] UpdateScoreRequest request)
    {
        try
        {
            var response = await _scoreService.UpdateScoreAsync(id, request);
            return Ok(new { message = "Cập nhật điểm thành công", scoreValue = response.ScoreValue });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // Xóa điểm số (Chỉ Admin và Giáo viên)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteScore(Guid id)
    {
        try
        {
            await _scoreService.DeleteScoreAsync(id);
            return Ok(new { message = "Xóa điểm số thành công" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
