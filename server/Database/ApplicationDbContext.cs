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

  public virtual DbSet<CustomProblem> CustomProblems { get; set; }
  public virtual DbSet<Enrollment> Enrollments { get; set; }
  public virtual DbSet<LeetcodeProblem> LeetcodeProblems { get; set; }
  public virtual DbSet<LeetcodeProblemCategory> LeetcodeProblemCategories { get; set; }
  public virtual DbSet<LeetcodeProblemDifficulty> LeetcodeProblemDifficulties { get; set; }
  public virtual DbSet<Mentorship> Mentorships { get; set; }
  public virtual DbSet<Problem> Problems { get; set; }
  public virtual DbSet<Role> Roles { get; set; }
  public virtual DbSet<Season> Seasons { get; set; }
  public virtual DbSet<User> Users { get; set; }
}