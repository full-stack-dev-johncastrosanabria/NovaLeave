using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Application.Requests.Models;

namespace NovaLeave.Application.Requests.Queries;

public sealed class GetMyRequestDetailQueryHandler
{
    private readonly IApplicationDbContext _dbContext;

    public GetMyRequestDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserRequestDetail>> HandleAsync(string userId, Guid requestId, CancellationToken cancellationToken)
    {
        var request = _dbContext.VacationRequests
            .Where(candidate => candidate.Id == requestId && candidate.OwnerId == userId)
            .Select(candidate => new UserRequestDetail(
                candidate.Id,
                candidate.StartDate,
                candidate.EndDate,
                candidate.WorkingDays,
                candidate.Status,
                candidate.Reason,
                candidate.RejectionReason,
                candidate.RowVersion,
                candidate.CreatedAtUtc,
                candidate.UpdatedAtUtc))
            .SingleOrDefault();

        var result = request is null
            ? Result<UserRequestDetail>.Failure(new Error(ErrorCodes.NotFound, "Solicitud no encontrada."))
            : Result<UserRequestDetail>.Success(request);
        return await Task.FromResult(result);
    }
}
