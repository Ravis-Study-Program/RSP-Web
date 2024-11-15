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

  public virtual DbSet<BehaviouralMockInterviewRoundEntity> BehaviouralMockInterviewRounds { get; set; }
  public virtual DbSet<CustomMockInterviewRoundEntity> CustomMockInterviewRounds { get; set; }
  public virtual DbSet<CustomProblemEntity> CustomProblems { get; set; }
  public virtual DbSet<EnrollmentEntity> Enrollments { get; set; }
  public virtual DbSet<LeetcodeMockInterviewRoundEntity> LeetcodeMockInterviewRounds { get; set; }
  public virtual DbSet<LeetcodeProblemEntity> LeetcodeProblems { get; set; }
  public virtual DbSet<LeetcodeProblemCategoryEntity> LeetcodeProblemCategories { get; set; }
  public virtual DbSet<MentorshipEntity> Mentorships { get; set; }
  public virtual DbSet<MockInterviewEntity> MockInterviews { get; set; }
  public virtual DbSet<MockInterviewRoundEntity> MockInterviewRounds { get; set; }
  public virtual DbSet<ProblemEntity> Problems { get; set; }
  public virtual DbSet<ProblemAttemptEntity> ProblemAttempts { get; set; }
  public virtual DbSet<SeasonEntity> Seasons { get; set; }
  public virtual DbSet<UserEntity> Users { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    modelBuilder.Entity<BehaviouralMockInterviewRoundEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<CustomMockInterviewRoundEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<CustomProblemEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<EnrollmentEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<LeetcodeMockInterviewRoundEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<LeetcodeProblemEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<LeetcodeProblemCategoryEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<MentorshipEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<MockInterviewEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<MockInterviewRoundEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<ProblemEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<ProblemAttemptEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<SeasonEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<UserEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);

    base.OnModelCreating(modelBuilder);
  }
}
