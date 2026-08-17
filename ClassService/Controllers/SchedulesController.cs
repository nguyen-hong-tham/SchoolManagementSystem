using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ClassService.Services.Interfaces;
using ClassService.DTOs.Schedules;

namespace ClassService.Controllers;

[ApiController]
[Route("api")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpPost("schedules")]
    public async Task<ActionResult<ScheduleResponseDto>> CreateSchedule([FromBody] CreateScheduleDto dto)
    {
        try
        {
            var result = await _scheduleService.CreateScheduleAsync(dto);
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

    [HttpPut("schedules/{id:guid}")]
    public async Task<ActionResult<ScheduleResponseDto>> UpdateSchedule(Guid id, [FromBody] UpdateScheduleDto dto)
    {
        try
        {
            var result = await _scheduleService.UpdateScheduleAsync(id, dto);
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

    [HttpDelete("schedules/{id:guid}")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        try
        {
            await _scheduleService.DeleteScheduleAsync(id);
            return Ok(new { message = "Xóa tiết học khỏi thời khóa biểu thành công." });
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

    [HttpDelete("classes/{classId:guid}/schedule")]
    public async Task<IActionResult> ClearClassSchedule(Guid classId, [FromQuery] string? schoolYear)
    {
        try
        {
            await _scheduleService.ClearClassScheduleAsync(classId, schoolYear);
            return Ok(new { message = "Xóa toàn bộ thời khóa biểu thành công." });
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

    [HttpGet("classes/{classId:guid}/schedule")]
    public async Task<ActionResult<IEnumerable<ScheduleResponseDto>>> GetClassSchedule(Guid classId, [FromQuery] string? schoolYear)
    {
        try
        {
            var result = await _scheduleService.GetClassScheduleAsync(classId, schoolYear);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("teachers/{teacherId:guid}/schedule")]
    public async Task<ActionResult<IEnumerable<ScheduleResponseDto>>> GetTeacherSchedule(Guid teacherId, [FromQuery] string? schoolYear)
    {
        var result = await _scheduleService.GetTeacherScheduleAsync(teacherId, schoolYear);
        return Ok(result);
    }
}
