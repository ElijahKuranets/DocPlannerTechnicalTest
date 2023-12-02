using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocPlanner.Filters;

public class SwaggerDateTimeFormatSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(DateTime) || context.Type == typeof(DateTime?))
        {
            schema.Format = "date-time";
            schema.Example = new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
        }
    }
}