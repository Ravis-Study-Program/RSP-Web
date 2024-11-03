using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSPWebAPI.Database;

namespace RSPWebAPI.Entities;

public class SeasonEntity
{
  public string SeasonId { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Slug { get; set; } = string.Empty;
  public DateTime StartDateInclusiveUtc { get; set; }
  public DateTime EndDateInclusiveUtc { get; set; }
  public string Location { get; set; } = string.Empty;
  public string ImageUrl { get; set; } = string.Empty;
}

public class SeasonEntityConfiguration : IEntityTypeConfiguration<SeasonEntity>
{
  public void Configure(EntityTypeBuilder<SeasonEntity> builder)
  {
    builder.ToTable("Season");
    builder.HasKey(x => x.SeasonId);

    // Indexes
    builder.HasIndex(x => x.Slug).IsUnique();

    // Fields
    builder.Property(x => x.SeasonId).HasColumnName("SeasonId").HasColumnType("varchar(16)")
           .ValueGeneratedNever().IsRequired();
    builder.Property(x => x.Name).HasColumnName("Name").HasColumnType("varchar(100)")
           .IsRequired();
    builder.Property(x => x.Slug).HasColumnName("Slug").HasColumnType("varchar(32)")
           .IsRequired();
    builder.Property(x => x.StartDateInclusiveUtc).HasColumnName("StartDateInclusiveUTC")
           .HasColumnType("timestamp").IsRequired();
    builder.Property(x => x.EndDateInclusiveUtc).HasColumnName("EndDateInclusiveUTC")
           .HasColumnType("timestamp").IsRequired();
    builder.Property(x => x.Location).HasColumnName("Location").HasColumnType("varchar(100)").IsRequired();
    builder.Property(x => x.ImageUrl).HasColumnName("ImageUrl").HasColumnType("varchar(255)").IsRequired();
  }
}
