using NovaLeave.Application.Requests.Models;

namespace NovaLeave.Web.ViewModels.MisSolicitudes;

public sealed class CreateVacationRequestViewModel
{
    public RequestInputMode InputMode { get; set; } = RequestInputMode.DateRange;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? WorkingDays { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int AccruedDays { get; set; }

    public int ReservedDays { get; set; }

    public int AvailableDays { get; set; }
}
