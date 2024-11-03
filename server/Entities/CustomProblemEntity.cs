using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RSPWebAPI.Entities;

public class CustomProblemEntity
{
  public string? CustomProblemId { get; set; } = string.Empty;
  public string? ProblemId { get; set; } = string.Empty;
  public string Difficulty { get; set; } = string.Empty;
  public string Question { get; set; } = string.Empty;

  // Navigation
  public virtual ProblemEntity? Problem { get; set; }
}

public class CustomProblemEntityConfiguration : IEntityTypeConfiguration<CustomProblemEntity>
{
  public void Configure(EntityTypeBuilder<CustomProblemEntity> builder)
  {
    builder.ToTable("CustomProblem");
    builder.HasKey(x => x.CustomProblemId);

    // Fields
    builder.Property(x => x.CustomProblemId).HasColumnName("CustomProblemId").HasColumnType("varchar(16)")
           .ValueGeneratedNever().IsRequired();
    builder.Property(x => x.ProblemId).HasColumnName("ProblemId").HasColumnType("varchar(16)")
           .IsRequired();
    builder.Property(x => x.Difficulty).HasColumnName("Difficulty").HasColumnType("varchar(50)")
           .IsRequired();
    builder.Property(x => x.Question).HasColumnName("Question").HasColumnType("varchar(255)")
           .IsRequired();

    // Foreign Keys
    builder.HasOne(x => x.Problem)
           .WithOne()
           .HasForeignKey<CustomProblemEntity>(x => x.ProblemId)
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired();
  }
}
