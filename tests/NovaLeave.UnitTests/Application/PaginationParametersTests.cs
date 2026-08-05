using NovaLeave.Application.Common.Models;

namespace NovaLeave.UnitTests.Application;

public sealed class PaginationParametersTests
{
    [Theory]
    [InlineData(-5, 0, 0, 1, 1, 1)]
    [InlineData(1, 50, 51, 1, 50, 2)]
    [InlineData(int.MaxValue, int.MaxValue, 401, 3, 200, 3)]
    public void Normalize_Bounds_Page_And_PageSize_Using_Server_Total(
        int page,
        int pageSize,
        int totalCount,
        int expectedPage,
        int expectedPageSize,
        int expectedTotalPages)
    {
        var result = PaginationParameters.Normalize(page, pageSize, totalCount);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    [Fact]
    public void PagedResult_Exposes_Authoritative_Navigation_Metadata()
    {
        var result = new PagedResult<int>([51], Page: 2, PageSize: 50, TotalCount: 51);

        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }
}
