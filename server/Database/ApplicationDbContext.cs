using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database.Helpers;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Database;

public class ApplicationDbContext : DbContext
{
  private readonly IHttpContextAccessor? _httpContextAccessor;

  public ApplicationDbContext() { }

  public ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IHttpContextAccessor httpContextAccessor
  ) : base(options)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public virtual DbSet<BehaviouralMockInterviewRoundEntity> BehaviouralMockInterviewRounds { get; set; }
  public virtual DbSet<CustomMockInterviewRoundEntity> CustomMockInterviewRounds { get; set; }
  public virtual DbSet<CustomProblemEntity> CustomProblems { get; set; }
  public virtual DbSet<EnrollmentEntity> Enrollments { get; set; }
  public virtual DbSet<LeetcodeMockInterviewRoundEntity> LeetcodeMockInterviewRounds { get; set; }
  public virtual DbSet<LeetcodeProblemEntity> LeetcodeProblems { get; set; }
  public virtual DbSet<LeetcodeProblemCategoryEntity> LeetcodeProblemCategories { get; set; }
  public virtual DbSet<LeetcodeProblemRecommendationEntity> LeetcodeProblemRecommendations { get; set; }
  public virtual DbSet<MentorshipEntity> Mentorships { get; set; }
  public virtual DbSet<MockInterviewEntity> MockInterviews { get; set; }
  public virtual DbSet<MockInterviewRoundEntity> MockInterviewRounds { get; set; }
  public virtual DbSet<ProblemEntity> Problems { get; set; }
  public virtual DbSet<ProblemAttemptEntity> ProblemAttempts { get; set; }
  public virtual DbSet<SeasonEntity> Seasons { get; set; }
  public virtual DbSet<UserEntity> Users { get; set; }
  public virtual DbSet<SeasonWeekEntity> SeasonWeeks { get; set; }
  public virtual DbSet<KickStudentEventEntity> KickStudentEvents { get; set; }
  public virtual DbSet<AuditEventEntity> AuditEvents { get; set; }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    string userId =
      _httpContextAccessor?.HttpContext?.User.FindFirst($"{Constants.Domain}userId")?.Value
      ?? "SYSTEM";

    return await SaveChangesWithAuditAsync(userId, cancellationToken);
  }

  public virtual async Task<int> SaveChangesWithAuditAsync(
    string userId,
    CancellationToken cancellationToken = default
  )
  {
    var changeSet = ChangeTracker.Entries().ToList();
    var auditEntries = await AuditHelper.GenerateAuditEntries(userId, changeSet);

    var result = await base.SaveChangesAsync(cancellationToken);

    try
    {
      await base.AddRangeAsync(auditEntries, cancellationToken);
      await base.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      // Audit logging shouldn't effect result of original query
      Console.WriteLine($"Failed to write Audit Entry. {ex.Message}");
    }

    return result;
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    modelBuilder
      .Entity<BehaviouralMockInterviewRoundEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder
      .Entity<CustomMockInterviewRoundEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder
      .Entity<CustomProblemEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<EnrollmentEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder
      .Entity<LeetcodeMockInterviewRoundEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder
      .Entity<LeetcodeProblemEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder
      .Entity<LeetcodeProblemCategoryEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder
      .Entity<LeetcodeProblemRecommendationEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<MentorshipEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder
      .Entity<MockInterviewEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder
      .Entity<MockInterviewRoundEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<ProblemEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder
      .Entity<ProblemAttemptEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<SeasonEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder.Entity<UserEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false && !x.IsTestUser);
    modelBuilder.Entity<SeasonWeekEntity>().HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);
    modelBuilder
      .Entity<KickStudentEventEntity>()
      .HasQueryFilter(x => x.DeletedAtUtc.HasValue == false);

    base.OnModelCreating(modelBuilder);
  }
}
