using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult GetSecret()
    {
        return Ok("Bạn đã đăng nhập");
    }
}