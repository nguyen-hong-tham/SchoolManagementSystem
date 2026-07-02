using Microsoft.EntityFrameworkCore;
using SubjectService.Entities;

namespace SubjectService.Data;

public class SubjectDbContext : DbContext
{
    public SubjectDbContext(DbContextOptions<SubjectDbContext> options)
        : base(options) { }

    public DbSet<Subject> Subjects => Set<Subject>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("subjects");

        // Tạo Index cho Code để tối ưu tốc độ tìm kiếm môn học
        modelBuilder.Entity<Subject>().HasIndex(s => s.Code).IsUnique();
    }
}
