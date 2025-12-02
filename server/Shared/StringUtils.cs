using System.Text.RegularExpressions;
using Bogus;

namespace RSPWebAPI.Shared.Strings
{
  public static class StringUtils
  {
    private static readonly Faker _faker = new Faker();
    

    public static string Slugify()
    {
      var color = _faker.Commerce.Color().ToLowerInvariant();
      var material = _faker.Commerce.ProductMaterial().ToLowerInvariant();
      
      color = Regex.Replace(color, @"[^a-z0-9]", "");
      material = Regex.Replace(material, @"[^a-z0-9]", "");
      
      return $"{color}-{material}";
    }
  }
}
