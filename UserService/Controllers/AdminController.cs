using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;

    public AdminController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _authService.GetUsers();
        return Ok(users);
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _authService.GetUserById(id);
        return Ok(user);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(AdminCreateUserRequest request)
    {
        var user = await _authService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
    {
        var user = await _authService.UpdateUser(id, request);
        return Ok(user);
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _authService.DeleteUser(id);
        return Ok(new { message = "User deleted successfully" });
    }

    [HttpPut("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, UpdateRoleRequest request)
    {
        await _authService.UpdateRole(id, request);
        return Ok(new { message = "Role updated successfully" });
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _authService.GetStudents();
        return Ok(students);
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers()
    {
        var teachers = await _authService.GetTeachers();
        return Ok(teachers);
    }
}
