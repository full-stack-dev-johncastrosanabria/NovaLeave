namespace NovaLeave.Application.Approvals.ApproveRequest;

public sealed record ApproveRequestCommand(string ApproverId, Guid RequestId, byte[] RowVersion);
