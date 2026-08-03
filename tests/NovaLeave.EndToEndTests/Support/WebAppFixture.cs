using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NovaLeave.EndToEndTests.Support;

/// <summary>
/// Hosts the real application on a Kestrel port so a browser can drive it.
/// <see cref="WebApplicationFactory{TEntryPoint}"/> alone serves requests in memory, which a
/// browser cannot reach, so the host is started explicitly and its bound address captured.
/// </summary>
public sealed class WebAppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IHost? _host;

    /// <summary>Absolute base address, for example <c>http://127.0.0.1:5123</c>.</summary>
    public string BaseAddress { get; private set; } = string.Empty;

    /// <summary>Connection string for the browser-facing database, or null when unavailable.</summary>
    public static string? TestConnectionString =>
        Environment.GetEnvironmentVariable("NOVALEAVE_TEST_SQLSERVER");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionString ?? string.Empty);
        builder.UseSetting("NovaLeave:PendingRequestTimeoutDays", "14");
        builder.UseSetting("NovaLeave:SessionTimeoutMinutes", "30");
        builder.UseSetting("NovaLeave:AccrualSchedulerCadence", "Daily 00:05 UTC");
        builder.UseSetting("NovaLeave:TimeoutSchedulerCadence", "Daily 00:05 UTC");
        // Demo identities give the browser something to sign in with.
        builder.UseSetting("NovaLeave:SeedDemoUsers", "true");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Kestrel on a free port, so Playwright can reach the app over real HTTP.
        builder.ConfigureWebHost(webHost => webHost.UseKestrel().UseUrls("http://127.0.0.1:0"));

        _host = builder.Build();
        _host.Start();

        var addresses = _host.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>();
        BaseAddress = addresses?.Addresses.FirstOrDefault() ?? string.Empty;

        // The base class still needs its own in-memory host for HttpClient-based helpers.
        var testHost = base.CreateHost(builder);
        return testHost;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        await base.DisposeAsync();
    }
}
