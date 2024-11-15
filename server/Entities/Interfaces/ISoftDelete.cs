namespace RSPWebAPI.Entities.Interfaces;

public interface ISoftDelete
{
  public DateTime? DeletedAtUtc { get; set; }
}
