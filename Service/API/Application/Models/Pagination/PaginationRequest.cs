namespace Staging.API.Application.Models.Pagination;

public sealed record PaginationRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; }
    public int PageSize { get; }

    public int Offset => (Page - 1) * PageSize;

    public PaginationRequest(int page, int pageSize)
    {
        Page = page < 1 ? 1 : page;

        PageSize = pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };
    }
}
