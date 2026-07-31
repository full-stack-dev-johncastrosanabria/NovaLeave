using NovaLeave.Application.Requests.Models;

namespace NovaLeave.Web.ViewModels.MisSolicitudes;

public sealed class EditVacationRequestViewModel
{
    public Guid Id { get; set; }

    public RequestInputMode InputMode { get; set; } = RequestInputMode.DateRange;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? WorkingDays { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string RowVersion { get; set; } = string.Empty;
}
