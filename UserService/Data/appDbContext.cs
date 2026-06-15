using Microsoft.EntityFrameworkCore;
using UserService.Entities;

namespace UserService.Data; // file nay thuộc nhóm UserService/Data

public class AppDbContext : DbContext // kế thừa từ DbContext của Entity Framework Core
{
    // nhận cấu hình database từ builder.Service.AddDbContext trong Program.cs
    public AppDbContext // quản lts db của UserService
    (DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>(); // entity User -> tables User
    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasOne(u => u.TeacherProfile)
            .WithOne(p => p.User)
            .HasForeignKey<TeacherProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
