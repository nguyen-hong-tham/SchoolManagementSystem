using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ScoreService.Consumers;
using ScoreService.Data;
using ScoreService.Middleware;
using ScoreService.Repositories;
using ScoreService.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình kết nối PostgreSQL
builder.Services.AddDbContext<ScoreDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w =>
        w.Ignore(
            Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning
        )
    );
});

// 2. Đăng ký Repository & Service
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();
builder.Services.AddScoped<IScoreService, ScoreService.Services.ScoreService>();

// 3. Đăng ký MassTransit & Consumers
builder.Services.AddMassTransit(x =>
{
    // Đăng ký các Consumer của người dùng
    x.AddConsumer<UserCreatedConsumer>();
    x.AddConsumer<UserUpdatedConsumer>();
    x.AddConsumer<UserDeletedConsumer>();

    // Đăng ký các Consumer của môn học
    x.AddConsumer<SubjectCreatedConsumer>();
    x.AddConsumer<SubjectUpdatedConsumer>();
    x.AddConsumer<SubjectDeletedConsumer>();

    x.UsingRabbitMq(
        (context, cfg) =>
        {
            cfg.Host(
                builder.Configuration["MessageBroker:Host"] ?? "localhost",
                "/",
                h =>
                {
                    h.Username(builder.Configuration["MessageBroker:Username"] ?? "guest");
                    h.Password(builder.Configuration["MessageBroker:Password"] ?? "guest");
                }
            );

            // Thiết lập Queue nhận tin người dùng
            cfg.ReceiveEndpoint(
                "score-service-user-events",
                e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureConsumer<UserCreatedConsumer>(context);
                    e.ConfigureConsumer<UserUpdatedConsumer>(context);
                    e.ConfigureConsumer<UserDeletedConsumer>(context);
                }
            );

            // Thiết lập Queue nhận tin môn học
            cfg.ReceiveEndpoint(
                "score-service-subject-events",
                e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureConsumer<SubjectCreatedConsumer>(context);
                    e.ConfigureConsumer<SubjectUpdatedConsumer>(context);
                    e.ConfigureConsumer<SubjectDeletedConsumer>(context);
                }
            );
        }
    );
});

// 4. Cấu hình JWT Bearer Authentication
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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

var app = builder.Build();

// 5. Cài đặt Exception Middleware toàn cục
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Link chạy: http://localhost:5221/scalar/v1
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Tự động Migration database trên môi trường Development
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ScoreDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi xảy ra trong quá trình khởi tạo hoặc cập nhật cơ sở dữ liệu.");
    }
}

app.Run();
