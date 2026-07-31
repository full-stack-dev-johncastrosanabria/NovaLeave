using System.Reflection;

namespace NovaLeave.UnitTests.Architecture;

public sealed class CleanArchitectureTests
{
    [Fact]
    public void Domain_Does_Not_Reference_Application_Infrastructure_Or_Web()
    {
        var references = Assembly.Load("NovaLeave.Domain")
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.DoesNotContain("NovaLeave.Application", references);
        Assert.DoesNotContain("NovaLeave.Infrastructure", references);
        Assert.DoesNotContain("NovaLeave.Web", references);
        Assert.DoesNotContain(references, name => name is not null && name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name is not null && name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
    }

    [Fact]
    public void Application_Does_Not_Reference_Infrastructure_Or_Web()
    {
        var references = Assembly.Load("NovaLeave.Application")
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Contains("NovaLeave.Domain", references);
        Assert.DoesNotContain("NovaLeave.Infrastructure", references);
        Assert.DoesNotContain("NovaLeave.Web", references);
    }

    [Fact]
    public void Infrastructure_References_Only_Inward_Application_And_Domain_Projects()
    {
        var projectFile = Path.Combine(FindRepositoryRoot(), "src", "NovaLeave.Infrastructure", "NovaLeave.Infrastructure.csproj");
        var projectXml = File.ReadAllText(projectFile);

        Assert.Contains("NovaLeave.Application.csproj", projectXml);
        Assert.Contains("NovaLeave.Domain.csproj", projectXml);
        Assert.DoesNotContain("NovaLeave.Web.csproj", projectXml);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "NovaLeave.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
