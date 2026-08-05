namespace NovaLeave.Application.Common.Models;

public sealed record PaginationParameters(int Page, int PageSize, int TotalPages)
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    public int Offset => (Page - 1) * PageSize;

    public static PaginationParameters Normalize(int page, int pageSize, int totalCount)
    {
        var normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var normalizedTotalCount = Math.Max(0, totalCount);
        var totalPages = Math.Max(1, (int)Math.Ceiling(normalizedTotalCount / (double)normalizedPageSize));
        var normalizedPage = Math.Clamp(page, 1, totalPages);

        return new PaginationParameters(normalizedPage, normalizedPageSize, totalPages);
    }
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(Math.Max(0, TotalCount) / (double)Math.Max(1, PageSize)));

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}
