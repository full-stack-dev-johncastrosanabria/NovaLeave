using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLeave.Application.Balances.Queries;
using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Requests.CreateVacationRequest;
using NovaLeave.Application.Requests.EditPendingRequest;
using NovaLeave.Application.Requests.Models;
using NovaLeave.Application.Requests.Queries;
using NovaLeave.Web.ViewModels.MisSolicitudes;

namespace NovaLeave.Web.Controllers;

[Authorize(Policy = "RequireActiveUser")]
public sealed class MisSolicitudesController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly GetMyRequestsQueryHandler _getMyRequests;
    private readonly GetMyRequestDetailQueryHandler _getMyRequestDetail;
    private readonly GetMyBalanceQueryHandler _getMyBalance;
    private readonly CreateVacationRequestHandler _createVacationRequest;
    private readonly EditPendingRequestHandler _editPendingRequest;

    public MisSolicitudesController(
        ICurrentUser currentUser,
        GetMyRequestsQueryHandler getMyRequests,
        GetMyRequestDetailQueryHandler getMyRequestDetail,
        GetMyBalanceQueryHandler getMyBalance,
        CreateVacationRequestHandler createVacationRequest,
        EditPendingRequestHandler editPendingRequest)
    {
        _currentUser = currentUser;
        _getMyRequests = getMyRequests;
        _getMyRequestDetail = getMyRequestDetail;
        _getMyBalance = getMyBalance;
        _createVacationRequest = createVacationRequest;
        _editPendingRequest = editPendingRequest;
    }

    [HttpGet("/mis-solicitudes")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var requests = await _getMyRequests.HandleAsync(userId, cancellationToken);
        return View(new MisSolicitudesIndexViewModel(requests));
    }

    [HttpGet("/mis-solicitudes/crear")]
    public IActionResult Create()
    {
        return View(new CreateVacationRequestViewModel());
    }

    [HttpPost("/mis-solicitudes/crear")]
    public async Task<IActionResult> Create(CreateVacationRequestViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return View(viewModel);
        }

        var command = new CreateVacationRequestCommand(
            RequireUserId(),
            viewModel.InputMode,
            viewModel.StartDate,
            viewModel.EndDate,
            viewModel.WorkingDays,
            viewModel.Reason);

        var result = await _createVacationRequest.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Code is ErrorCodes.NotFound or ErrorCodes.Forbidden)
            {
                return ToActionResult(result.Error);
            }

            return WorkflowError(viewModel, result.Error);
        }

        return RedirectToAction(nameof(Detail), new { id = result.Value });
    }

    [HttpGet("/mis-solicitudes/{id:guid}")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getMyRequestDetail.HandleAsync(RequireUserId(), id, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound();
        }

        return View(new SolicitudDetalleViewModel(result.Value!));
    }

    [HttpGet("/mis-solicitudes/{id:guid}/editar")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getMyRequestDetail.HandleAsync(RequireUserId(), id, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var request = result.Value!;
        return View(new EditVacationRequestViewModel
        {
            Id = request.Id,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            RowVersion = Convert.ToBase64String(request.RowVersion)
        });
    }

    [HttpPost("/mis-solicitudes/{id:guid}/editar")]
    public async Task<IActionResult> Edit(Guid id, EditVacationRequestViewModel viewModel, CancellationToken cancellationToken)
    {
        viewModel.Id = id;
        if (!ModelState.IsValid)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return View(viewModel);
        }

        byte[] rowVersion;
        try
        {
            rowVersion = Convert.FromBase64String(viewModel.RowVersion);
        }
        catch (FormatException)
        {
            ModelState.AddModelError(string.Empty, ConflictMessage);
            Response.StatusCode = StatusCodes.Status409Conflict;
            return View(viewModel);
        }

        var command = new EditPendingRequestCommand(
            RequireUserId(),
            id,
            viewModel.InputMode,
            viewModel.StartDate,
            viewModel.EndDate,
            viewModel.WorkingDays,
            viewModel.Reason,
            rowVersion);

        var result = await _editPendingRequest.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Code is ErrorCodes.NotFound or ErrorCodes.Forbidden)
            {
                return ToActionResult(result.Error);
            }

            return WorkflowError(viewModel, result.Error);
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpGet("/saldo")]
    public async Task<IActionResult> Balance(CancellationToken cancellationToken)
    {
        var result = await _getMyBalance.HandleAsync(RequireUserId(), cancellationToken);
        if (result.IsFailure)
        {
            return NotFound();
        }

        return View("~/Views/MisSolicitudes/Balance.cshtml", new SaldoViewModel(result.Value!));
    }

    private string RequireUserId()
    {
        return _currentUser.UserId ?? throw new InvalidOperationException("Usuario autenticado requerido.");
    }

    private IActionResult ToActionResult(Error? error)
    {
        return error?.Code switch
        {
            ErrorCodes.Conflict => Conflict(error.Message),
            ErrorCodes.NotFound => NotFound(),
            ErrorCodes.Forbidden => Forbid(),
            _ => BadRequest(error?.Message ?? "Solicitud invalida.")
        };
    }

    private IActionResult WorkflowError(object viewModel, Error? error)
    {
        var isConflict = error?.Code == ErrorCodes.Conflict;
        ModelState.AddModelError(string.Empty, isConflict ? ConflictMessage : error?.Message ?? "No fue posible completar la solicitud.");
        Response.StatusCode = isConflict ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
        return View(viewModel);
    }

    private const string ConflictMessage = "La informacion cambio mientras realizaba la operacion. Actualice la pagina e intentelo nuevamente.";
}
