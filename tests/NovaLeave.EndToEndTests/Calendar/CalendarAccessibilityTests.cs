namespace NovaLeave.EndToEndTests.Calendar;

public sealed class CalendarAccessibilityTests
{
    [Fact]
    public void Shared_Calendar_Partial_Contains_Keyboard_And_Accessibility_Hooks()
    {
        var root = FindRepositoryRoot();
        var partialPath = Path.Combine(root, "src", "NovaLeave.Web", "Views", "Shared", "_Calendar.cshtml");
        var partial = File.ReadAllText(partialPath);

        Assert.Contains("role=\"grid\"", partial);
        Assert.Contains("role=\"gridcell\"", partial);
        Assert.Contains("data-calendar-keyboard=\"true\"", partial);
        Assert.Contains("tabindex=\"0\"", partial);
        Assert.Contains("aria-label", partial);
        Assert.Contains("Solicitud", partial);
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
