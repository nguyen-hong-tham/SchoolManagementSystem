using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Events;
using SubjectService.DTOs;
using SubjectService.Entities;
using SubjectService.Repositories;

namespace SubjectService.Controllers;

[ApiController]
[Route("api/subjects")]
[Authorize] // Bắt buộc đăng nhập với mọi endpoint theo mặc định
public class SubjectController : ControllerBase
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public SubjectController(ISubjectRepository subjectRepository, IPublishEndpoint publishEndpoint)
    {
        _subjectRepository = subjectRepository;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet] // Lấy danh sách môn học (Student, Teacher, Admin đều được gọi)
    public async Task<ActionResult<IEnumerable<SubjectResponse>>> GetAll(
        [FromQuery] int? gradeLevel
    )
    {
        var subjects = await _subjectRepository.GetAllSubjectsAsync();

        if (gradeLevel.HasValue)
        {
            subjects = subjects.Where(s => s.GradeLevel == gradeLevel.Value);
        }

        var response = subjects
            .Select(s => new SubjectResponse
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                GradeLevel = s.GradeLevel,
                CreatedAt = s.CreatedAt,
            })
            .ToList();

        return Ok(response);
    }

    // Lấy chi tiết môn học theo id
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubjectResponse>> GetSubject(Guid id)
    {
        var subject = await _subjectRepository.GetSubjectByIdAsync(id);
        if (subject == null)
        {
            return NotFound(new { message = $"Không tìm thấy môn học với Id: {id}" });
        }

        var response = new SubjectResponse
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Description = subject.Description,
            GradeLevel = subject.GradeLevel,
            CreatedAt = subject.CreatedAt,
        };

        return Ok(response);
    }

    // Tạo môn học mới (Chỉ Admin mới có quyền)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubjectResponse>> CreateSubject(
        [FromBody] CreateSubjectRequest request
    )
    {
        // Kiểm tra xem môn học đã tồn tại chưa
        var exists = await _subjectRepository.GetSubjectByCodeAsync(request.Code);
        if (exists != null)
        {
            return BadRequest(new { message = "Môn học đã tồn tại" });
        }

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Description = request.Description,
            GradeLevel = request.GradeLevel,
            CreatedAt = DateTime.UtcNow,
        };

        await _subjectRepository.CreateSubjectAsync(subject);
        await _subjectRepository.SaveChangesAsync();

        // Publish event to RabbitMQ
        await _publishEndpoint.Publish<SubjectCreatedEvent>(
            new
            {
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                GradeLevel = subject.GradeLevel,
            }
        );

        var response = new SubjectResponse
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Description = subject.Description,
            GradeLevel = subject.GradeLevel,
            CreatedAt = subject.CreatedAt,
        };

        return CreatedAtAction(nameof(GetSubject), new { id = subject.Id }, response);
    }

    // Cập nhật thông tin môn học (Chỉ Admin mới có quyền)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubjectRequest request)
    {
        var subject = await _subjectRepository.GetSubjectByIdAsync(id);
        if (subject == null)
        {
            return NotFound(new { message = $"Không tìm thấy môn học với Id: {id}" });
        }

        subject.Name = request.Name;
        subject.Description = request.Description;
        subject.GradeLevel = request.GradeLevel;

        await _subjectRepository.UpdateSubjectAsync(subject);
        await _subjectRepository.SaveChangesAsync();

        // Publish event to RabbitMQ
        await _publishEndpoint.Publish<SubjectUpdatedEvent>(
            new
            {
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                GradeLevel = subject.GradeLevel,
            }
        );

        return Ok(
            new SubjectResponse
            {
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                Description = subject.Description,
                GradeLevel = subject.GradeLevel,
                CreatedAt = subject.CreatedAt,
            }
        );
    }

    // Xóa môn học (Chỉ Admin mới có quyền)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var subject = await _subjectRepository.GetSubjectByIdAsync(id);
        if (subject == null)
        {
            return NotFound(new { message = $"Không tìm thấy môn học với Id: {id}" });
        }

        await _subjectRepository.DeleteSubjectAsync(id);
        await _subjectRepository.SaveChangesAsync();

        // Publish event to RabbitMQ
        await _publishEndpoint.Publish<SubjectDeletedEvent>(new { Id = id });

        return Ok(new { message = "Xóa môn học thành công" });
    }
}
