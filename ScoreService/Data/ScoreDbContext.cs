using Microsoft.EntityFrameworkCore;
using ScoreService.Entities;

namespace ScoreService.Data;

public class ScoreDbContext : DbContext
{
    public ScoreDbContext(DbContextOptions<ScoreDbContext> options)
        : base(options) { }

    public DbSet<Score> Scores { get; set; }
    public DbSet<CachedUser> CachedUsers { get; set; }
    public DbSet<CachedSubject> CachedSubjects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("scores");

        modelBuilder
            .Entity<Score>()
            .HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Score>()
            .HasOne(s => s.Subject)
            .WithMany()
            .HasForeignKey(s => s.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ràng buộc điểm
        modelBuilder
            .Entity<Score>()
            .ToTable(t =>
                t.HasCheckConstraint(
                    "CK_Score_Points",
                    "\"ScoreValue\" >= 0.0 AND \"ScoreValue\" <= 10.0"
                )
            );
    }
}
