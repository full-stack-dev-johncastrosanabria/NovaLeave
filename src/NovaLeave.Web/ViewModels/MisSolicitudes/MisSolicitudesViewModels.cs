using NovaLeave.Application.Balances.Queries;
using NovaLeave.Application.Requests.Models;

namespace NovaLeave.Web.ViewModels.MisSolicitudes;

public sealed record MisSolicitudesIndexViewModel(
    string UserDisplayName,
    IReadOnlyList<UserRequestSummary> Requests,
    MyBalanceView Balance);

public sealed record SolicitudDetalleViewModel(UserRequestDetail Request);

public sealed record SaldoViewModel(string UserDisplayName, MyBalanceView Balance);
