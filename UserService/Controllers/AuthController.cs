using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace UserService.Controllers;

[ApiController] //Đây là API Controller : validate + convert JSON + 400
[Route("api/[controller]")] // route
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]// endpoint


    public async Task<IActionResult> Register(RegisterRequest request)// asp.net nhận json và tự map vào 
    {
      var result = await _authService.RegisterAsync(request);
      return Ok(result);
    }

    [HttpPost("login")]
    public async Task <IActionResult> Login (LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new 
        {
            Id = userId,
            Email = email,
            Role = role
        });
    }

   

}