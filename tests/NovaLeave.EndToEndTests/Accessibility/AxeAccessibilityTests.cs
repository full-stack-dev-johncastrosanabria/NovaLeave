using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using NovaLeave.EndToEndTests.Support;

namespace NovaLeave.EndToEndTests.Accessibility;

/// <summary>
/// Runs axe-core against the rendered application so WCAG 2.1 AA conformance is verified rather
/// than assumed. The constitution is explicit that Bootstrap does not guarantee accessibility and
/// every screen must still be reviewed (§11.2); these tests make the machine-checkable part of
/// that review automatic.
/// </summary>
/// <remarks>
/// Requires Playwright browsers. They are a large download and are not installed by default, so
/// each test skips with an explanatory message when they are missing, and the suite stays green
/// on a machine that has not opted in. Install them with:
/// <c>pwsh tests/NovaLeave.EndToEndTests/bin/Debug/net10.0/playwright.ps1 install chromium</c>
/// or <c>npx playwright install chromium</c>.
/// </remarks>
public sealed class AxeAccessibilityTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public AxeAccessibilityTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    public static TheoryData<string, bool> AuditedPages => new()
    {
        // path, requiresSignIn
        { "/Identity/Account/Login", false },
        { "/mis-solicitudes", true },
        { "/mis-solicitudes/crear", true },
        { "/saldo", true },
        { "/calendario?context=User", true }
    };

    [SkippableTheory]
    [MemberData(nameof(AuditedPages))]
    public async Task Page_Has_No_Serious_Or_Critical_Accessibility_Violations(string path, bool requiresSignIn)
    {
        Skip.If(
            WebAppFixture.TestConnectionString is null,
            "NOVALEAVE_TEST_SQLSERVER is not set; the browser host needs a database.");

        using var playwright = await TryCreatePlaywrightAsync();
        await using var browser = await TryLaunchAsync(playwright);

        var page = await browser.NewPageAsync();
        await page.GotoAsync(_fixture.BaseAddress + "/Identity/Account/Login");

        if (requiresSignIn)
        {
            await page.FillAsync("input[name='Input.Email']", "multi@demo");
            await page.FillAsync("input[name='Input.Password']", "Demo123!");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForURLAsync(url => !url.Contains("/Identity/Account/Login", StringComparison.Ordinal));
            await page.GotoAsync(_fixture.BaseAddress + path);
        }
        else
        {
            await page.GotoAsync(_fixture.BaseAddress + path);
        }

        var results = await page.RunAxe(new AxeRunOptions
        {
            RunOnly = new RunOnlyOptions { Type = "tag", Values = ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa"] }
        });

        var blocking = results.Violations
            .Where(v => v.Impact is "serious" or "critical")
            .ToArray();

        Assert.True(
            blocking.Length == 0,
            $"axe-core found {blocking.Length} serious/critical violation(s) on {path}:\n" +
            string.Join("\n", blocking.Select(v =>
                $"  [{v.Impact}] {v.Id}: {v.Help} ({v.Nodes.Length} node(s)) - {v.HelpUrl}")));
    }

    private static async Task<IPlaywright> TryCreatePlaywrightAsync()
    {
        try
        {
            return await Microsoft.Playwright.Playwright.CreateAsync();
        }
        catch (PlaywrightException exception)
        {
            Skip.If(true,$"Playwright is unavailable: {exception.Message}");
            throw; // unreachable; keeps the compiler satisfied
        }
    }

    private static async Task<IBrowser> TryLaunchAsync(IPlaywright playwright)
    {
        try
        {
            // Channel "chromium" runs the full browser in the new headless mode, so only the
            // standard `playwright install chromium` download is required; without it Playwright
            // looks for the separate chromium_headless_shell build.
            return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Channel = "chromium"
            });
        }
        catch (PlaywrightException exception)
        {
            Skip.If(true,$"Chromium is not installed; run 'playwright install chromium'. {exception.Message}");
            throw; // unreachable
        }
    }
}
