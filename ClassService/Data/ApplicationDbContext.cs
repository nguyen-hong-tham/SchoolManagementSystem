using ClassService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Class> Classes { get; set; }

    public DbSet<StudentClass> StudentClasses { get; set; }

    public DbSet<HomeroomAssignment> HomeroomAssignments { get; set; }

    public DbSet<TeachingAssignment> TeachingAssignments { get; set; }

    public DbSet<Schedule> Schedules { get; set; }

    public DbSet<CachedUser> CachedUsers { get; set; }

    public DbSet<CachedSubject> CachedSubjects { get; set; }
}
