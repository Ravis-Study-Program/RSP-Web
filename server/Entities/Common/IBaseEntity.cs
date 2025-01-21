namespace RSPWebAPI.Entities.Interfaces;

public interface IBaseEntity
{
  public DateTime CreatedAtUtc { get; set; }
  public DateTime UpdatedAtUtc { get; set; }
}
