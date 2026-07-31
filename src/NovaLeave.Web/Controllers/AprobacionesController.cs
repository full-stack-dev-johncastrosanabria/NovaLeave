using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLeave.Application.Approvals.ApproveRequest;
using NovaLeave.Application.Approvals.History;
using NovaLeave.Application.Approvals.Queries;
using NovaLeave.Application.Approvals.RejectRequest;
using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Web.ViewModels.Aprobaciones;

namespace NovaLeave.Web.Controllers;

[Authorize(Policy = "RequireActiveApprover")]
public sealed class AprobacionesController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly GetApproverQueueQueryHandler _getQueue;
    private readonly GetApproverRequestDetailQueryHandler _getDetail;
    private readonly GetApproverHistoryQueryHandler _getHistory;
    private readonly ApproveRequestHandler _approve;
    private readonly RejectRequestHandler _reject;

    public AprobacionesController(
        ICurrentUser currentUser,
        GetApproverQueueQueryHandler getQueue,
        GetApproverRequestDetailQueryHandler getDetail,
        GetApproverHistoryQueryHandler getHistory,
        ApproveRequestHandler approve,
        RejectRequestHandler reject)
    {
        _currentUser = currentUser;
        _getQueue = getQueue;
        _getDetail = getDetail;
        _getHistory = getHistory;
        _approve = approve;
        _reject = reject;
    }

    [HttpGet("/aprobaciones")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _getQueue.HandleAsync(RequireUserId(), cancellationToken);
        return result.IsFailure
            ? ToActionResult(result.Error)
            : View(new AprobacionesIndexViewModel(result.Value!));
    }

    [HttpGet("/aprobaciones/{id:guid}")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getDetail.HandleAsync(RequireUserId(), id, cancellationToken);
        if (result.IsFailure)
        {
            return ToActionResult(result.Error);
        }

        return View(new AprobacionDetalleViewModel(result.Value!, Convert.ToBase64String(result.Value!.RowVersion)));
    }

    [HttpPost("/aprobaciones/{id:guid}/aprobar")]
    public async Task<IActionResult> Approve(Guid id, string rowVersion, CancellationToken cancellationToken)
    {
        if (!TryDecode(rowVersion, out var decoded))
        {
            return BadRequest("RowVersion no valida.");
        }

        var result = await _approve.HandleAsync(new ApproveRequestCommand(RequireUserId(), id, decoded), cancellationToken);
        return result.IsFailure ? ToActionResult(result.Error) : RedirectToAction(nameof(History));
    }

    [HttpPost("/aprobaciones/{id:guid}/rechazar")]
    public async Task<IActionResult> Reject(Guid id, RechazarSolicitudViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!TryDecode(viewModel.RowVersion, out var decoded))
        {
            return BadRequest("RowVersion no valida.");
        }

        var result = await _reject.HandleAsync(new RejectRequestCommand(RequireUserId(), id, viewModel.RejectionReason, decoded), cancellationToken);
        return result.IsFailure ? ToActionResult(result.Error) : RedirectToAction(nameof(History));
    }

    [HttpGet("/aprobaciones/historial")]
    public async Task<IActionResult> History(CancellationToken cancellationToken)
    {
        var result = await _getHistory.HandleAsync(RequireUserId(), cancellationToken);
        return result.IsFailure
            ? ToActionResult(result.Error)
            : View(new AprobacionesHistorialViewModel(result.Value!));
    }

    private string RequireUserId()
    {
        return _currentUser.UserId ?? throw new InvalidOperationException("Usuario autenticado requerido.");
    }

    private static bool TryDecode(string rowVersion, out byte[] decoded)
    {
        try
        {
            decoded = Convert.FromBase64String(rowVersion);
            return true;
        }
        catch (FormatException)
        {
            decoded = [];
            return false;
        }
    }

    private IActionResult ToActionResult(Error? error)
    {
        return error?.Code switch
        {
            ErrorCodes.Conflict => Conflict(error.Message),
            ErrorCodes.NotFound => NotFound(),
            ErrorCodes.Forbidden => Forbid(),
            ErrorCodes.Validation => BadRequest(error.Message),
            _ => BadRequest(error?.Message ?? "Solicitud invalida.")
        };
    }
}
