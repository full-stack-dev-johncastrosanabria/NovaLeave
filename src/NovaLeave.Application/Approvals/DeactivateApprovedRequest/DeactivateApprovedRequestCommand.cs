namespace NovaLeave.Application.Approvals.DeactivateApprovedRequest;

public sealed record DeactivateApprovedRequestCommand(string ApproverId, Guid RequestId, byte[] RowVersion);
