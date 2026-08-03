using FluentValidation;
using NovaLeave.Application.Requests.EditPendingRequest;
using NovaLeave.Application.Requests.Models;

namespace NovaLeave.Application.Requests.Validation;

public sealed class EditPendingRequestValidator : AbstractValidator<EditPendingRequestCommand>
{
    public EditPendingRequestValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.RowVersion).NotEmpty();
        RuleFor(command => command.StartDate).NotEmpty();
        RuleFor(command => command.Reason)
            .NotEmpty()
            .Must(reason => !string.IsNullOrWhiteSpace(reason) && reason.Trim().Length is >= 10 and <= 500)
            .WithMessage("El motivo debe contener entre 10 y 500 caracteres.");
        RuleFor(command => command.WorkingDays)
            .GreaterThan(0)
            .When(command => command.InputMode == RequestInputMode.StartPlusDays);
        RuleFor(command => command.EndDate)
            .NotNull()
            .When(command => command.InputMode == RequestInputMode.DateRange);
    }
}
