using Microsoft.AspNetCore.Mvc;
using ClassService.Services.Interfaces;
using ClassService.DTOs.Classes;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Controllers;

[ApiController]
[Route("api/classes")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
    {
        _classService = classService;
    }



    [HttpPost]
    public async Task<ActionResult<ClassResponseDto>> CreateAsync(CreateClassDto request)
    {
        try
        {
            var classes = await _classService.CreateAsync(request);
            return CreatedAtAction("GetById", new { id = classes.Id }, classes);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClassResponseDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var classes = await _classService.GetByIdAsync(id);
            if (classes == null)
            {
                return NotFound();
            }
            return Ok(classes);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClassResponseDto>>> GetAllAsync()
    {
        var classes = await _classService.GetAllAsync();
        return Ok(classes);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClassResponseDto>> UpdateAsync(Guid id, UpdateClassDto request)
    {
        try
        {
            var classes = await _classService.UpdateAsync(id, request);
            if (classes == null)
            {
                return NotFound();
            }
            return Ok(classes);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ClassResponseDto>> DeleteAsync(Guid id)
    {
        try
        {
            var classes = await _classService.DeleteAsync(id);
            if (classes == null)
            {
                return NotFound();
            }
            return Ok(classes);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Không tìm thấy lớp học với ID: {id}" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
