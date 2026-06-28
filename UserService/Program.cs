using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using UserService.Data;
using UserService.Consumers;
using UserService.Middleware;
using UserService.Repositories;
using UserService.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<StudentPromotedConsumer>();

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

            cfg.ReceiveEndpoint(
                "user-service-student-events",
                e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, System.TimeSpan.FromSeconds(5)));
                    e.ConfigureConsumer<StudentPromotedConsumer>(context);
                }
            );
        }
    );
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập JWT Token",
        }
    );

    options.AddSecurityRequirement(document =>
        new() { [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>() }
    );
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w =>
        w.Ignore(
            Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning
        )
    );
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtService>();

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
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi khi seed dữ liệu cho UserService.");
    }
}

app.Run();
