using System.Runtime.Serialization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RSPWebAPI.Shared;

public class EnumSchemaFilter : ISchemaFilter
{
  public void Apply(OpenApiSchema schema, SchemaFilterContext context)
  {
    if (context.Type.IsEnum)
    {
      schema.Enum.Clear();
      schema.Type = "integer";
      schema.Format = "int32";

      var enumNames = new OpenApiArray();
      var enumValues = new OpenApiArray();
      var uniqueEnumValues = new HashSet<int>();
      var uniqueEnumNames = new HashSet<string>();

      foreach (var enumValue in Enum.GetValues(context.Type))
      {
        var memberInfo = context.Type.GetMember(enumValue?.ToString() ?? "")[0];
        var enumMemberAttribute = memberInfo
          .GetCustomAttributes(typeof(EnumMemberAttribute), false)
          .Cast<EnumMemberAttribute>()
          .FirstOrDefault();

        var name = enumMemberAttribute?.Value ?? enumValue?.ToString();
        var intValue = enumValue != null ? (int)enumValue : -1;

        // Add to enumValues only if the value is unique
        if (uniqueEnumValues.Add(intValue))
        {
          enumValues.Add(new OpenApiInteger(intValue));
        }

        // Add to enumNames only if the name is unique
        if (uniqueEnumNames.Add(name))
        {
          enumNames.Add(new OpenApiString(name));
        }
      }

      schema.Enum = enumValues;
      schema.Extensions.Add("x-enumNames", enumNames);
    }
  }
}
