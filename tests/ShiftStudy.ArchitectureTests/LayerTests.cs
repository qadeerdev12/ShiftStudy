using System.Reflection;
using AwesomeAssertions;
using NetArchTest.Rules;

namespace ShiftStudy.ArchitectureTests;

public class LayerTests
{
    private static readonly Assembly Domain = typeof(Domain.AssemblyMarker).Assembly;
    private static readonly Assembly Scheduling = typeof(Scheduling.AssemblyMarker).Assembly;
    private static readonly Assembly Application = typeof(Application.AssemblyMarker).Assembly;

    [Fact]
    public void Scheduling_references_only_the_BCL_and_NodaTime()
    {
        var allowed = new[] { "System", "Microsoft.CSharp", "NodaTime" };

        var references = Scheduling.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => name != "netstandard" && !allowed.Any(p => name == p || name.StartsWith(p + ".", StringComparison.Ordinal)));

        references.Should().BeEmpty();
    }

    [Fact]
    public void Scheduling_does_not_depend_on_other_ShiftStudy_layers()
    {
        var result = Types.InAssembly(Scheduling).ShouldNot()
            .HaveDependencyOnAny("ShiftStudy.Domain", "ShiftStudy.Application", "ShiftStudy.Infrastructure", "ShiftStudy.Api", "ShiftStudy.Web")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_does_not_depend_on_outer_layers()
    {
        var result = Types.InAssembly(Domain).ShouldNot()
            .HaveDependencyOnAny("ShiftStudy.Application", "ShiftStudy.Infrastructure", "ShiftStudy.Api", "ShiftStudy.Web", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_does_not_depend_on_Infrastructure_or_Api()
    {
        var result = Types.InAssembly(Application).ShouldNot()
            .HaveDependencyOnAny("ShiftStudy.Infrastructure", "ShiftStudy.Api", "ShiftStudy.Web", "Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
