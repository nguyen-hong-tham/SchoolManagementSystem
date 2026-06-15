using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserService.Entities;

namespace UserService.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
{
    Console.WriteLine("Jwt:Key = " + _configuration["Jwt:Key"]);
    Console.WriteLine("Jwt:Issuer = " + _configuration["Jwt:Issuer"]);

    var key = _configuration["Jwt:Key"];

    if (string.IsNullOrEmpty(key))
    {
        throw new Exception("JWT Key đang NULL");
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier,
            user.Id.ToString()),

        new Claim(ClaimTypes.Email,
            user.Email),

        new Claim(ClaimTypes.Role,
            user.Role.ToString())
    };

    var securityKey =
        new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

    var credentials =
        new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

    var token =
        new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

    return new JwtSecurityTokenHandler()
        .WriteToken(token);
}
}