using DocPlanner.Filters;
using FluentAssertions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace DocPlanner.UnitTests.Filters;

public class SwaggerDateTimeFormatSchemaFilterTests
{
    [Theory]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTime?))]
    public void Apply_FormatAndExampleSetForDateTimeTypes(Type type)
    {
        // Arrange
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(type, null, null); 
        var filter = new SwaggerDateTimeFormatSchemaFilter();

        // Act
        filter.Apply(schema, context);

        // Assert
        schema.Format.Should().Be("date-time");
        schema.Example.Should().BeOfType<OpenApiString>();
        var example = (OpenApiString)schema.Example;
        example.Value.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
    }

    [Fact]
    public void Apply_NoModificationForNonDateTimeTypes()
    {
        // Arrange
        var schema = new OpenApiSchema();
        var context = new SchemaFilterContext(typeof(string), null, null);
        var filter = new SwaggerDateTimeFormatSchemaFilter();

        // Act
        filter.Apply(schema, context);

        // Assert
        schema.Format.Should().BeNull();
        schema.Example.Should().BeNull();
    }
}