using System.ComponentModel.DataAnnotations;
using NovaLeave.Application.Requests.Models;

namespace NovaLeave.Web.ViewModels.MisSolicitudes;

public abstract class VacationRequestFormViewModel
{
    public RequestInputMode InputMode { get; set; } = RequestInputMode.DateRange;

    [Required(ErrorMessage = "Indique la fecha de inicio.")]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? WorkingDays { get; set; }

    [Required(ErrorMessage = "Explique el motivo de la solicitud.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "El motivo debe contener entre 10 y 500 caracteres.")]
    public string Reason { get; set; } = string.Empty;

    public int? AvailableDays { get; set; }
}
