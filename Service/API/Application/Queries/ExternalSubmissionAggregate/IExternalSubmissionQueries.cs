using Staging.API.Application.Common.Pagination;
using Staging.API.Application.Dtos;
using Staging.API.Application.Models.Pagination;

namespace Staging.API.Application.Queries.ExternalSubmissionAggregate;

public interface IExternalSubmissionQueries
{
    Task<PagedResult<ExternalSubmissionDto>> GetExternalSubmissionsForDataFlagAsync(
            int packageEventDataFlagId,
            PaginationRequest pagination);
}
