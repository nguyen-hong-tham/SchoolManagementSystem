using FrontendMVC.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();

// Register HttpClients with token propagation
builder
    .Services.AddHttpClient(
        "UserService",
        client =>
        {
            var baseAddress = builder.Configuration["Microservices:UserService"]!;
            client.BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/");
        }
    )
    .AddHttpMessageHandler<BearerTokenHandler>();

builder
    .Services.AddHttpClient(
        "ClassService",
        client =>
        {
            var baseAddress = builder.Configuration["Microservices:ClassService"]!;
            client.BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/");
        }
    )
    .AddHttpMessageHandler<BearerTokenHandler>();

builder
    .Services.AddHttpClient(
        "SubjectService",
        client =>
        {
            var baseAddress = builder.Configuration["Microservices:SubjectService"]!;
            client.BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/");
        }
    )
    .AddHttpMessageHandler<BearerTokenHandler>();

builder
    .Services.AddHttpClient(
        "ScoreService",
        client =>
        {
            var baseAddress = builder.Configuration["Microservices:ScoreService"]!;
            client.BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/");
        }
    )
    .AddHttpMessageHandler<BearerTokenHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
