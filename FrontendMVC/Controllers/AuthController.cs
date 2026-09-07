using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using FrontendMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FrontendMVC.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public AuthController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Nếu đã có Token và Role hợp lệ, điều hướng dựa theo Role
        var token = Request.Cookies["jwt_token"];
        var role = Request.Cookies["user_role"];

        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(role))
        {
            if (role == "Admin")
                return RedirectToAction("Dashboard", "Admin");
            if (role == "Teacher")
                return RedirectToAction("Dashboard", "Teacher");
            if (role == "Student")
                return RedirectToAction("Dashboard", "Student");
        }

        // Nếu token không tồn tại nhưng cookie role/name vẫn còn sót lại, dọn sạch
        if (!string.IsNullOrEmpty(role) || !string.IsNullOrEmpty(token))
        {
            var clearOptions = new CookieOptions { Path = "/" };
            Response.Cookies.Delete("jwt_token", clearOptions);
            Response.Cookies.Delete("user_role", clearOptions);
            Response.Cookies.Delete("user_id", clearOptions);
            Response.Cookies.Delete("user_name", clearOptions);
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _clientFactory.CreateClient("UserService");
        try
        {
            // Call API đăng nhập
            var response = await client.PostAsJsonAsync(
                "auth/login",
                new { usernameOrEmail = model.UsernameOrEmail, password = model.Password }
            );

            if (response.IsSuccessStatusCode)
            {
                var loginResult = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (loginResult != null && !string.IsNullOrEmpty(loginResult.Token))
                {
                    // Giải mã JWT để lưu Role và Name
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(loginResult.Token);

                    var role =
                        jwt.Claims.FirstOrDefault(c =>
                            c.Type == ClaimTypes.Role || c.Type == "role"
                        )?.Value
                        ?? "Student";
                    var nameId =
                        jwt.Claims.FirstOrDefault(c =>
                            c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid"
                        )?.Value
                        ?? string.Empty;
                    var fullName = loginResult.User?.FullName ?? "N/A";

                    var isHttps = Request.IsHttps || Request.Headers["X-Forwarded-Proto"] == "https";

                    // Lưu vào Cookie (HttpOnly cho Token để bảo mật, Lax để không bị rớt cookie)
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTime.UtcNow.AddDays(7),
                        Secure = isHttps,
                        SameSite = SameSiteMode.Lax,
                        Path = "/"
                    };
                    Response.Cookies.Append("jwt_token", loginResult.Token, cookieOptions);

                    var normalOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(7),
                        Secure = isHttps,
                        SameSite = SameSiteMode.Lax,
                        Path = "/"
                    };
                    Response.Cookies.Append("user_role", role, normalOptions);
                    Response.Cookies.Append("user_id", nameId, normalOptions);
                    Response.Cookies.Append("user_name", fullName, normalOptions);

                    // Chuyển hướng theo vai trò
                    if (role == "Admin")
                        return RedirectToAction("Dashboard", "Admin");
                    if (role == "Teacher")
                        return RedirectToAction("Dashboard", "Teacher");
                    if (role == "Student")
                        return RedirectToAction("Dashboard", "Student");
                }
            }
            else
            {
                var errorMsg = "Đăng nhập thất bại. Email/Username hoặc mật khẩu không chính xác.";
                try
                {
                    var errorObj = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    if (errorObj != null && !string.IsNullOrEmpty(errorObj.Message))
                    {
                        errorMsg = errorObj.Message;
                    }
                }
                catch { }
                ModelState.AddModelError(string.Empty, errorMsg);
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Không thể kết nối đến máy chủ xác thực.");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Logout()
    {
        var clearOptions = new CookieOptions { Path = "/" };
        Response.Cookies.Delete("jwt_token", clearOptions);
        Response.Cookies.Delete("user_role", clearOptions);
        Response.Cookies.Delete("user_id", clearOptions);
        Response.Cookies.Delete("user_name", clearOptions);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
