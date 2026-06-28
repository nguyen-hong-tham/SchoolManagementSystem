using ClassService.Consumers;
using ClassService.Data;
using ClassService.Repositories;
using ClassService.Repositories.Interfaces;
using ClassService.Services;
using ClassService.Services.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMassTransit(x =>
{
    // Đăng ký các Consumers
    x.AddConsumer<UserCreatedConsumer>();
    x.AddConsumer<UserUpdatedConsumer>();
    x.AddConsumer<UserDeletedConsumer>();
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

            // Tự động định cấu hình Endpoint (như tên hàng đợi - queue name) dựa trên tên các Consumer đã đăng ký
            cfg.ReceiveEndpoint(
                "class-service-user-events",
                e =>
                {
                    // Thiết lập retry tin nhắn 3 lần nếu có lỗi xảy ra trước khi đưa vào hàng đợi lỗi (error queue)
                    e.UseMessageRetry(r => r.Interval(3, System.TimeSpan.FromSeconds(5)));

                    e.ConfigureConsumer<UserCreatedConsumer>(context);
                    e.ConfigureConsumer<UserUpdatedConsumer>(context);
                    e.ConfigureConsumer<UserDeletedConsumer>(context);
                }
            );

            cfg.ReceiveEndpoint(
                "class-service-subject-events",
                e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, System.TimeSpan.FromSeconds(5)));

                    e.ConfigureConsumer<SubjectCreatedConsumer>(context);
                    e.ConfigureConsumer<SubjectUpdatedConsumer>(context);
                    e.ConfigureConsumer<SubjectDeletedConsumer>(context);
                }
            );
        }
    );
});

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IStudentClassRepository, StudentClassRepository>();
builder.Services.AddScoped<IHomeroomAssignmentRepository, HomeroomAssignmentRepository>();
builder.Services.AddScoped<ITeachingAssignmentRepository, TeachingAssignmentRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();

builder.Services.AddScoped<IClassService, ClassService.Services.ClassService>();
builder.Services.AddScoped<IStudentClassService, ClassService.Services.StudentClassService>();
builder.Services.AddScoped<
    IHomeroomAssignmentService,
    ClassService.Services.HomeroomAssignmentService
>();
builder.Services.AddScoped<
    ITeachingAssignmentService,
    ClassService.Services.TeachingAssignmentService
>();
builder.Services.AddScoped<IScheduleService, ClassService.Services.ScheduleService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await ClassService.Data.DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi khi seed dữ liệu cho ClassService.");
    }
}

app.Run();
