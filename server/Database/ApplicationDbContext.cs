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

  public virtual DbSet<Season> Seasons { get; set; }
  public virtual DbSet<User> Users { get; set; }
}