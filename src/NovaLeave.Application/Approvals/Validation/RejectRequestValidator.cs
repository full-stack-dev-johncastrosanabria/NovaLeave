using FluentValidation;
using NovaLeave.Application.Approvals.RejectRequest;

namespace NovaLeave.Application.Approvals.Validation;

public sealed class RejectRequestValidator : AbstractValidator<RejectRequestCommand>
{
    public RejectRequestValidator()
    {
        RuleFor(command => command.ApproverId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.RowVersion).NotEmpty();
        RuleFor(command => command.RejectionReason).NotEmpty().Must(value => value.Trim().Length is >= 10 and <= 500);
    }
}
