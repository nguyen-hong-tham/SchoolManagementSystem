using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _authService.GetUsers();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _authService.GetUserById(id);
        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequest request)
    {
        // Resource owner or Admin check
        var currentUserIdClaim = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;
        var currentUserRoleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (currentUserIdClaim != id.ToString() && currentUserRoleClaim != "Admin")
        {
            return Forbid("You are not allowed to update this profile.");
        }

        var updatedUser = await _authService.UpdateUser(id, request);
        return Ok(updatedUser);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // Admin only delete
        var currentUserRoleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (currentUserRoleClaim != "Admin")
        {
            return Forbid("Only admins can delete users.");
        }

        await _authService.DeleteUser(id);
        return Ok(new { message = "User deleted successfully" });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var currentUserIdClaim = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        if (
            string.IsNullOrEmpty(currentUserIdClaim)
            || !Guid.TryParse(currentUserIdClaim, out var userId)
        )
        {
            return Unauthorized("User is not authenticated properly.");
        }

        try
        {
            await _authService.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Password updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordDto request)
    {
        var currentUserRoleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(currentUserRoleClaim))
        {
            return Unauthorized("User is not authenticated properly.");
        }

        try
        {
            await _authService.ResetPasswordAsync(id, request, currentUserRoleClaim);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("internal/sync-class")]
    [AllowAnonymous]
    public async Task<IActionResult> SyncClassInternal([FromBody] SyncClassRequestDto request)
    {
        Console.WriteLine(
            $"[HTTP Sync Received] Syncing StudentId={request.StudentId}, NewClassId={request.NewClassId}, Status={request.StudentStatus}..."
        );
        try
        {
            await _authService.SyncClassAsync(
                request.StudentId,
                request.NewClassId,
                request.StudentStatus
            );
            Console.WriteLine(
                $"[HTTP Sync Success] StudentId={request.StudentId} successfully saved to user_db."
            );
            return Ok(new { message = "Class synced successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[HTTP Sync Error] Failed to process sync for StudentId={request.StudentId}: {ex.Message}"
            );
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class SyncClassRequestDto
{
    public Guid StudentId { get; set; }
    public Guid? NewClassId { get; set; }
    public string? StudentStatus { get; set; }
}
