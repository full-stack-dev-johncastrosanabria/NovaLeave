namespace NovaLeave.Application.Observability;

public sealed record BusinessInvariantResult(bool IsHealthy, IReadOnlyCollection<string> Violations)
{
    public static BusinessInvariantResult Healthy()
    {
        return new BusinessInvariantResult(true, Array.Empty<string>());
    }

    public static BusinessInvariantResult Unhealthy(IReadOnlyCollection<string> violations)
    {
        return new BusinessInvariantResult(false, violations);
    }
}
