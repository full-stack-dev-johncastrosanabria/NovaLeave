namespace NovaLeave.Web.ViewModels.MisSolicitudes;

public sealed class EditVacationRequestViewModel : VacationRequestFormViewModel
{
    public Guid Id { get; set; }

    public string RowVersion { get; set; } = string.Empty;
}
