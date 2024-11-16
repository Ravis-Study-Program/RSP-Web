using System.Text;

namespace RSPWebAPI.Database;

public static class Constants
{
  public const int GenerateIdLength = 10;

  private static readonly Random Random = new();

  public static string GeneratePrimaryKeyId()
  {
    const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
    var result = new StringBuilder(GenerateIdLength);

    for (var i = 0; i < GenerateIdLength; i++)
    {
      result.Append(chars[Random.Next(chars.Length)]);
    }

    return result.ToString();
  }
}
