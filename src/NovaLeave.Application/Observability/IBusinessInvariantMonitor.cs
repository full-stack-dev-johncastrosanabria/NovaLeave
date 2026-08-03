namespace NovaLeave.Application.Observability;

public interface IBusinessInvariantMonitor
{
    Task<BusinessInvariantResult> EvaluateAsync(CancellationToken cancellationToken);
}
