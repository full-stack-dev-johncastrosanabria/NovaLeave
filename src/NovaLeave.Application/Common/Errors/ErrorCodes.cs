namespace NovaLeave.Application.Common.Errors;

public static class ErrorCodes
{
    public const string Validation = "validation";
    public const string Forbidden = "forbidden";
    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string Unauthorized = "unauthorized";
    public const string InsufficientBalance = "insufficient_balance";
}

public sealed record Error(string Code, string Message)
{
    public static Error Validation(string message)
    {
        return new Error(ErrorCodes.Validation, message);
    }

    public static Error Forbidden(string message)
    {
        return new Error(ErrorCodes.Forbidden, message);
    }

    public static Error Conflict(string message)
    {
        return new Error(ErrorCodes.Conflict, message);
    }
}
