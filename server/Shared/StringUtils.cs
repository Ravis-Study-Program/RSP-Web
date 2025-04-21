using System.Text.RegularExpressions;

namespace server.Shared.Strings
{
  public static class StringUtils
  {
    public static string Slugify(string input)
    {
      input = input.ToLowerInvariant().Trim();
      input = Regex.Replace(input, @"[^a-z0-9\s-]", ""); // Remove non-alphanumerics
      input = Regex.Replace(input, @"\s+", "-"); // Replace spaces with dashes
      input = Regex.Replace(input, @"-+", "-"); // Remove multiple dashes
      return input;
    }
  }
}
