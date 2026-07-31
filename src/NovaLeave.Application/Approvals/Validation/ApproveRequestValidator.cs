using FluentValidation;
using NovaLeave.Application.Approvals.ApproveRequest;

namespace NovaLeave.Application.Approvals.Validation;

public sealed class ApproveRequestValidator : AbstractValidator<ApproveRequestCommand>
{
    public ApproveRequestValidator()
    {
        RuleFor(command => command.ApproverId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.RowVersion).NotEmpty();
    }
}
