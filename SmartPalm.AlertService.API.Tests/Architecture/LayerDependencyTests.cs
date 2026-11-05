using System.Reflection;
using SmartPalm.AlertService.Domain.Model.Aggregates;

namespace SmartPalm.AlertService.API.Tests.Architecture;

public class LayerDependencyTests
{
    [Fact]
    public void DomainPublicApi_DoesNotExposeOuterLayers()
    {
        var forbidden = new[] { ".Application", ".Infrastructure", ".Interfaces", ".Api" };
        var violations = typeof(Alert).Assembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith("SmartPalm.AlertService.Domain", StringComparison.Ordinal) == true)
            .SelectMany(ExposedTypes)
            .Where(type => forbidden.Any(segment => type.Namespace?.Contains(segment, StringComparison.Ordinal) == true))
            .Select(type => type.FullName).Distinct().ToArray();
        Assert.Empty(violations);
    }

    [Fact]
    public void CommandAndQueryTypes_UseDomainModelNamespaces()
    {
        var types = typeof(Alert).Assembly.GetTypes();
        var commands = types.Where(type => type.Name.EndsWith("Command", StringComparison.Ordinal)).ToArray();
        var queries = types.Where(type => type.Name.EndsWith("Query", StringComparison.Ordinal)).ToArray();
        Assert.NotEmpty(commands); Assert.NotEmpty(queries);
        Assert.All(commands, type => Assert.Equal("SmartPalm.AlertService.Domain.Model.Commands", type.Namespace));
        Assert.All(queries, type => Assert.Equal("SmartPalm.AlertService.Domain.Model.Queries", type.Namespace));
    }

    private static IEnumerable<Type> ExposedTypes(Type type)
    {
        foreach (var constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public))
        foreach (var parameter in constructor.GetParameters()) yield return Unwrap(parameter.ParameterType);
        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
        { yield return Unwrap(method.ReturnType); foreach (var parameter in method.GetParameters()) yield return Unwrap(parameter.ParameterType); }
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)) yield return Unwrap(property.PropertyType);
    }
    private static Type Unwrap(Type type) => type.IsArray ? type.GetElementType() ?? type : type.IsGenericType ? type.GetGenericArguments().FirstOrDefault() ?? type : type;
}
