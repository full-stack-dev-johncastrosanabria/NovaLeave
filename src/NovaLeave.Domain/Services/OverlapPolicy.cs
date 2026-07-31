using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Enums;
using NovaLeave.Domain.ValueObjects;

namespace NovaLeave.Domain.Services;

public sealed class OverlapPolicy
{
    public bool HasBlockingOverlap(IEnumerable<VacationRequest> existingRequests, DateRange candidate)
    {
        return existingRequests.Any(request =>
            request.Status is RequestStatus.Pending or RequestStatus.Approved &&
            request.DateRange.Overlaps(candidate));
    }
}
