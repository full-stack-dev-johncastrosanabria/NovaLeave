namespace NovaLeave.EndToEndTests.Accessibility;

public sealed class AccessibilityRegressionTests
{
    [Theory]
    [InlineData("src/NovaLeave.Web/Views/MisSolicitudes/Create.cshtml", "_ValidationSummary", "form-label")]
    [InlineData("src/NovaLeave.Web/Views/MisSolicitudes/Edit.cshtml", "_ValidationSummary", "form-label")]
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

    [Fact]
    public void Shared_Confirmation_Modal_Has_Accessible_Title_Description_And_Controls()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/Views/Shared/_ConfirmationModal.cshtml"));

        Assert.Contains("aria-labelledby=\"nlConfirmationTitle\"", content);
        Assert.Contains("aria-describedby=\"nlConfirmationMessage\"", content);
        Assert.Contains("data-confirm-submit", content);
        Assert.Contains("data-bs-dismiss=\"modal\"", content);
    }

    [Fact]
    public void Motion_System_Provides_Page_Entry_And_Reduced_Motion_Override()
    {
        var content = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src/NovaLeave.Web/wwwroot/css/site.css"));

        Assert.Contains(".nl-page-enter", content);
        Assert.Contains("translateY(4px)", content);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", content);
        Assert.Contains("animation-duration: 1ms !important", content);
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
