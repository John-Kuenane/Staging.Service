namespace Staging.API.Application.Models.Pagination;
public record PaginationMeta(
    int TotalItems,
    int PageIndex,
    int PageSize,
    int TotalPages
);