using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SubjectService.Data;
using SubjectService.Middleware;
using SubjectService.Repositories;
using SubjectService.Services;
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình kết nối PostgreSQL
builder.Services.AddDbContext<SubjectDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// 2. Đăng ký Repository & Service Pattern
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService.Services.SubjectService>();

// 2.1 Cấu hình MassTransit RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq(
        (context, cfg) =>
        {
            cfg.Host(
                builder.Configuration["MessageBroker:Host"] ?? "localhost",
                builder.Configuration["MessageBroker:VirtualHost"] ?? "/",
                h =>
                {
                    h.Username(builder.Configuration["MessageBroker:Username"] ?? "guest");
                    h.Password(builder.Configuration["MessageBroker:Password"] ?? "guest");
                    if (builder.Configuration.GetValue<bool>("MessageBroker:UseSsl"))
                    {
                        h.UseSsl(s => s.ServerName = builder.Configuration["MessageBroker:Host"]!);
                    }
                }
            );
        }
    );
});

// 3. Cấu hình JWT Bearer Authentication
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Hỗ trợ sinh tài liệu OpenAPI spec (.NET 9)

var app = builder.Build();

// 4. Sử dụng Exception Middleware toàn cục
app.UseMiddleware<ExceptionMiddleware>();

// Cấu hình tài liệu API Scalar khi chạy môi trường Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Link chạy: http://localhost:5187/scalar/v1
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 5. Tự động áp dụng Migration và Seed dữ liệu mẫu khi chạy ứng dụng
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SubjectDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi trong quá trình Migration và Seed dữ liệu.");
    }
}

app.Run();
