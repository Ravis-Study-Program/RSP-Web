using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Database;

public class ApplicationDbContext : DbContext
{
  public ApplicationDbContext()
  {
  }

  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
  {
  }

  public virtual DbSet<Enrollment> Enrollments { get; set; }
  public virtual DbSet<Mentorship> Mentorships { get; set; }
  public virtual DbSet<Role> Roles { get; set; }
  public virtual DbSet<Season> Seasons { get; set; }
  public virtual DbSet<User> Users { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Role>().HasData(
      new Role { RoleId = Guid.NewGuid(), Name = "Coordinator" },
      new Role { RoleId = Guid.NewGuid(), Name = "Mentor" },
      new Role { RoleId = Guid.NewGuid(), Name = "Student" }
    );
  }
}