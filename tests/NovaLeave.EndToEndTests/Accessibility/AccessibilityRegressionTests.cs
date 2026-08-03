namespace NovaLeave.EndToEndTests.Accessibility;

public sealed class AccessibilityRegressionTests
{
    [Theory]
    [InlineData("src/NovaLeave.Web/Views/MisSolicitudes/_RequestForm.cshtml", "_ValidationSummary", "form-label")]
    [InlineData("src/NovaLeave.Web/Views/Aprobaciones/Detail.cshtml", "aria-label", "form-label")]
    [InlineData("src/NovaLeave.Web/Views/RRHH/ApproverCapabilities/Capability.cshtml", "aria-labelledby", "form-label")]
    [InlineData("src/NovaLeave.Web/Views/Shared/_Calendar.cshtml", "role=\"grid\"", "aria-label")]
    public void Critical_Razor_Pages_Retain_Accessibility_Hooks(string relativePath, string requiredA, string requiredB)
    {
        var content = File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));

        Assert.Contains(requiredA, content);
        Assert.Contains(requiredB, content);
    }

    [Theory]
    [InlineData("src/NovaLeave.Web/Views/Shared/_StatusBadge.cshtml")]
    [InlineData("src/NovaLeave.Web/Views/Shared/_Calendar.cshtml")]
    public void Status_Indicators_Use_Text_Not_Color_Alone(string relativePath)
    {
        var content = File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));

        Assert.True(
            content.Contains("@Model", StringComparison.Ordinal) ||
            content.Contains("@label", StringComparison.Ordinal) ||
            content.Contains("StatusLabel", StringComparison.Ordinal),
            "Status indicators must render a visible text value, not only a CSS color.");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "NovaLeave.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
