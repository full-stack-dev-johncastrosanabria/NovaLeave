namespace NovaLeave.Application.Approvals.RejectRequest;

public sealed record RejectRequestCommand(string ApproverId, Guid RequestId, string RejectionReason, byte[] RowVersion);
