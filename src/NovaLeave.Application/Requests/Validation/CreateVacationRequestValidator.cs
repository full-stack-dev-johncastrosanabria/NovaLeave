using FluentValidation;
using NovaLeave.Application.Requests.CreateVacationRequest;
using NovaLeave.Application.Requests.Models;

namespace NovaLeave.Application.Requests.Validation;

public sealed class CreateVacationRequestValidator : AbstractValidator<CreateVacationRequestCommand>
{
    public CreateVacationRequestValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.StartDate).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(500);
        RuleFor(command => command.WorkingDays)
            .GreaterThan(0)
            .When(command => command.InputMode == RequestInputMode.StartPlusDays);
        RuleFor(command => command.EndDate)
            .NotNull()
            .When(command => command.InputMode == RequestInputMode.DateRange);
    }
}
