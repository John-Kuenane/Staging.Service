using Microsoft.Data.SqlClient;
using Staging.API.Application.Common.Pagination;
using Staging.API.Application.Dtos;
using Staging.API.Application.Models.Pagination;

namespace Staging.API.Application.Queries.ExternalSubmissionAggregate;

public class ExternalSubmissionQueries
    : IExternalSubmissionQueries
{
    private string _connectionString = string.Empty;

    public ExternalSubmissionQueries(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<PagedResult<ExternalSubmissionDto>> GetExternalSubmissionsForDataFlagAsync(
        int packageEventDataFlagId,
        PaginationRequest pagination)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $@"
        SELECT 
	        item.Id,
	        item.Vendor,
	        item.Payload,
	        item.[Status],
	        item.ExternalId,
	        item.[Message],
	        FORMAT (item.CreatedAt, 'yyyy-MM-dd hh:mm tt') AS Created,
	        FORMAT (item.LastAttemptAt, 'yyyy-MM-dd hh:mm tt') AS LastAttempt
        FROM staging.ExternalSubmission aggr
        INNER JOIN staging.ExternalSubmissionItems item ON item.ExternalSubmissionId = aggr.Id
        WHERE aggr.SourceType = 'DataFlag'
        AND aggr.SourceId = @PackageEventDataFlagId
        ORDER BY item.CreatedAt DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1)
        FROM staging.ExternalSubmission aggr
        INNER JOIN staging.ExternalSubmissionItems item ON item.ExternalSubmissionId = aggr.Id
        WHERE aggr.SourceType = 'DataFlag'
        AND aggr.SourceId = @PackageEventDataFlagId;
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            PackageEventDataFlagId = packageEventDataFlagId,
            Offset = pagination.Offset,
            PageSize = pagination.PageSize
        });

        var submissions = (await multi
            .ReadAsync<ExternalSubmissionDto>())
            .ToList();

        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<ExternalSubmissionDto>(
            submissions,
            totalCount,
            pagination.Page,
            pagination.PageSize);
    }
}