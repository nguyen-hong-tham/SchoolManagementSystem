using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassService.DTOs.HomeroomAssignments;
using ClassService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClassService.Controllers;

[ApiController]
[Route("api/classes")]
public class HomeroomAssignmentsController : ControllerBase
{
    private readonly IHomeroomAssignmentService _homeroomService;

    public HomeroomAssignmentsController(IHomeroomAssignmentService homeroomService)
    {
        _homeroomService = homeroomService;
    }

    [HttpPost("{classId:guid}/homeroom")]
    public async Task<ActionResult<HomeroomAssignmentResponseDto>> AssignHomeroom(
        Guid classId,
        [FromBody] AssignHomeroomDto dto
    )
    {
        try
        {
            var result = await _homeroomService.AssignHomeroomAsync(classId, dto);
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

    [HttpPut("{classId:guid}/homeroom")]
    public async Task<ActionResult<HomeroomAssignmentResponseDto>> ChangeHomeroom(
        Guid classId,
        [FromBody] AssignHomeroomDto dto
    )
    {
        try
        {
            var result = await _homeroomService.ChangeHomeroomAsync(classId, dto);
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

    [HttpGet("{classId:guid}/homeroom")]
    public async Task<ActionResult<HomeroomAssignmentResponseDto>> GetHomeroom(
        Guid classId,
        [FromQuery] string schoolYear
    )
    {
        var result = await _homeroomService.GetCurrentHomeroomAsync(classId, schoolYear);
        if (result == null)
        {
            return NotFound(new { message = "Lớp học chưa được phân công GVCN cho năm học này." });
        }
        return Ok(result);
    }

    [HttpGet("homerooms")]
    public async Task<ActionResult<IEnumerable<HomeroomAssignmentResponseDto>>> GetAllHomerooms([FromQuery] string? schoolYear)
    {
        var result = await _homeroomService.GetAllHomeroomsAsync(schoolYear);
        return Ok(result);
    }
}

