using System.Reflection;
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

  public virtual DbSet<CustomProblemEntity> CustomProblems { get; set; }
  public virtual DbSet<EnrollmentEntity> Enrollments { get; set; }
  public virtual DbSet<LeetcodeProblemEntity> LeetcodeProblems { get; set; }
  public virtual DbSet<LeetcodeProblemCategoryEntity> LeetcodeProblemCategories { get; set; }
  public virtual DbSet<MentorshipEntity> Mentorships { get; set; }
  public virtual DbSet<MockInterviewEntity> MockInterviews { get; set; }
  public virtual DbSet<ProblemEntity> Problems { get; set; }
  public virtual DbSet<ProblemAttemptEntity> ProblemAttempts { get; set; }
  public virtual DbSet<SeasonEntity> Seasons { get; set; }
  public virtual DbSet<UserEntity> Users { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    base.OnModelCreating(modelBuilder);
  }
}
