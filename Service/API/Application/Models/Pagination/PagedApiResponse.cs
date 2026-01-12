namespace Staging.API.Application.Models.Pagination;

public record PagedApiResponse<T>(
    IReadOnlyList<T> Value,
    int RecordCount,
    PaginationMeta Pagination
);