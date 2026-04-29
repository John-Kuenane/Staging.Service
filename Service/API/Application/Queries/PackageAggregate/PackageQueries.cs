using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Staging.API.Application.Common.Filters;
using Staging.API.Application.Common.Pagination;
using Staging.API.Application.Dtos;
using Staging.API.Application.Models.Pagination;

namespace Staging.API.Application.Queries.PackageAggregate;

public class PackageQueries
    : IPackageQueries
{
    private string _connectionString = string.Empty;
    private readonly IMemoryCache _cache;

    public PackageQueries(string connectionString, IMemoryCache cache)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
        _cache = cache;
    }

    public async Task<PackageForManagementDto> GetPackageForManagementAsync(int packageId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var sql = $@"
					SELECT	pac.Id,
							CASE 
								WHEN pac.PackageTypeId = 1 THEN 'Data Listing' 
								WHEN pac.PackageTypeId = 2 THEN 'Data Collection' 
								WHEN pac.PackageTypeId = 3 THEN 'Enrollment' 
								WHEN pac.PackageTypeId = 4 THEN 'Digital Payment Enrollment' 
								WHEN pac.PackageTypeId = 5 THEN 'Community Validation' 
							END AS 'PackageType',
							pac.OrgUnitId,
							pac.ParentOrgUnitName,
							pac.UniqueCode,
							pac.[Description],
							FORMAT (pac.Created, 'yyyy-MM-dd hh:mm tt') AS Created,
			                (SELECT COUNT(*) FROM staging.PackageEvent evt INNER JOIN staging.PackageEventHousehold hh ON evt.Id = hh.PackageEventId WHERE evt.PackageId = {packageId}) AS NumberHouseholds,
			                (SELECT COUNT(*) FROM staging.PackageEvent evt INNER JOIN staging.PackageEventHousehold hh ON evt.Id = hh.PackageEventId WHERE evt.PackageId = {packageId} AND hh.CollectionStatusId = 2) AS NumberEnumerations,
			                (SELECT COUNT(*) FROM staging.PackageEvent evt INNER JOIN staging.PackageEventDataFlag flag ON evt.Id = flag.PackageEventId WHERE evt.PackageId = {packageId}) AS NumberDataFlags,
			                0 AS NumberCompletedSubPackages,
                            pac.FormId
					FROM [staging].[Package] pac
					WHERE pac.Id = {packageId}";

            var package = await connection.QuerySingleAsync<PackageForManagementDto>(sql);

            if(package.FormId.HasValue)
            {
                var form = await LoadFormAsync(package.FormId.Value, connection);
                package.Forms.Add(form);
            }

            return package;
        }
    }

    public async Task<DataFlagForDetailDto> GetPackageEventDataFlagAsync(int packageEventDataFlagId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $@"
        SELECT
	        Id,
	        CASE 
		        WHEN flag.DataFlagTypeId = 1 THEN 'Data Quality Issue' 
		        ELSE 'UKNOWN'
	        END AS DataFlagType,
	        CASE 
		        WHEN flag.DataFlagSubTypeId = 1 THEN 'Custom' 
		        WHEN flag.DataFlagSubTypeId = 2 THEN 'PMT Correction Required' 
		        WHEN flag.DataFlagSubTypeId = 3 THEN 'PMT Screening Required' 
		        WHEN flag.DataFlagSubTypeId = 4 THEN 'NICR LookUp Failed' 
		        ELSE 'UKNOWN'
	        END AS DataFlagSubType,
	        [Subject],
	        CASE 
		        WHEN flag.FlagResolved_Status = 0 AND flag.FlagDeferred_Status = 0 THEN 'Open' 
		        WHEN flag.FlagResolved_Status = 1 THEN 'Resolved' 
		        WHEN flag.FlagDeferred_Status = 1 THEN 'Deferred' 
	        END AS 'Status',
	        Requester_FullName AS RequesterName,
	        Requester_Email AS RequesterEmail,
	        [Description],
	        CASE 
		        WHEN flag.PriorityId = 1 THEN 'Low' 
		        WHEN flag.PriorityId = 2 THEN 'Medium' 
		        WHEN flag.PriorityId = 3 THEN 'High' 
		        WHEN flag.PriorityId = 4 THEN 'Urgent' 
	        END AS 'Priority',
	        CASE 
		        WHEN flag.GroupId = 1 THEN 'Field Managers' 
		        WHEN flag.GroupId = 2 THEN 'IT Support' 
		        WHEN flag.GroupId = 3 THEN 'MoGYSD' 
		        WHEN flag.GroupId = 4 THEN 'QA/QC' 
		        WHEN flag.GroupId = 5 THEN 'Vendor' 
	        END AS 'Group',
	        FORMAT (flag.Created, 'yyyy-MM-dd hh:mm tt') AS Created,
	        FORMAT (flag.FlagResolved_ChangeDate, 'yyyy-MM-dd hh:mm tt') AS Resolved
        FROM staging.PackageEventDataFlag flag
		WHERE flag.Id = {packageEventDataFlagId}";

        return await connection.QuerySingleAsync<DataFlagForDetailDto>(sql);
    }

    public async Task<PackageEventHouseholdSynchForManagementDto> GetLatestSynchForHouseholdAsync(
        int packageEventId,
        int packageEventHouseholdId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $@"
        SELECT	
        TOP 1 
        syn.Id,
		syn.DeviceId,
		FORMAT (syn.Created, 'yyyy-MM-dd hh:mm tt') AS Created,
		syn.Payload,
		CASE 
			WHEN syn.PayloadProcessedId = 1 THEN 'Not Processed'
			WHEN syn.PayloadProcessedId = 2 THEN 'Processed'
			WHEN syn.PayloadProcessedId = 3 THEN 'Failed'
			WHEN syn.PayloadProcessedId = 4 THEN 'Pending'
			WHEN syn.PayloadProcessedId = 5 THEN 'Processing'
		END AS ProcessedStatus,
		FORMAT (syn.PayloadProcessedDate, 'yyyy-MM-dd hh:mm tt') AS ProcessedDate
        FROM [staging].[PackageEventHousehold] hh
        INNER JOIN [staging].[PackageEventHouseholdSynch] syn ON hh.Id = syn.PackageEventHouseholdId
        WHERE hh.PackageEventId = {packageEventId} 
	    AND hh.Id = {packageEventHouseholdId}
        ORDER BY syn.PayloadProcessedDate DESC";

        var packageEventHouseholdSynch = await connection.QuerySingleAsync<PackageEventHouseholdSynchForManagementDto>(sql);

        return packageEventHouseholdSynch;
    }

    public async Task<IEnumerable<PackageForManagementDto>> GetPackagesForManagementAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            const string sql = @"
                WITH HouseholdCounts AS (
                    SELECT evt.PackageId,
                           COUNT(hh.Id)                                                    AS NumberHouseholds,
                           SUM(CASE WHEN hh.CollectionStatusId = 2 THEN 1 ELSE 0 END)     AS NumberEnumerations
                    FROM [staging].[PackageEvent] evt
                    INNER JOIN [staging].[PackageEventHousehold] hh ON hh.PackageEventId = evt.Id
                    GROUP BY evt.PackageId
                ),
                FlagCounts AS (
                    SELECT evt.PackageId,
                           COUNT(flag.Id) AS NumberDataFlags
                    FROM [staging].[PackageEvent] evt
                    INNER JOIN [staging].[PackageEventDataFlag] flag ON flag.PackageEventId = evt.Id
                    GROUP BY evt.PackageId
                )
                SELECT pac.Id,
                       CASE
                           WHEN pac.PackageTypeId = 1 THEN 'Data Listing'
                           WHEN pac.PackageTypeId = 2 THEN 'Data Collection'
                           WHEN pac.PackageTypeId = 3 THEN 'Enrollment'
                           WHEN pac.PackageTypeId = 4 THEN 'Digital Payment Enrollment'
                           WHEN pac.PackageTypeId = 5 THEN 'Community Validation'
                       END AS PackageType,
                       pac.OrgUnitId,
                       pac.ParentOrgUnitName,
                       pac.UniqueCode,
                       pac.[Description],
                       FORMAT(pac.Created, 'yyyy-MM-dd hh:mm tt') AS Created,
                       ISNULL(hc.NumberHouseholds, 0)   AS NumberHouseholds,
                       ISNULL(hc.NumberEnumerations, 0) AS NumberEnumerations,
                       ISNULL(fc.NumberDataFlags, 0)    AS NumberDataFlags,
                       0                                AS NumberCompletedSubPackages,
                       pac.FormId
                FROM [staging].[Package] pac
                LEFT JOIN HouseholdCounts hc ON hc.PackageId = pac.Id
                LEFT JOIN FlagCounts      fc ON fc.PackageId = pac.Id";

            return await connection.QueryAsync<PackageForManagementDto>(sql);
        }
    }

    public async Task<IEnumerable<PackageDto>> GetCollectionPackagesForDeviceAsync(string deviceId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var packages = await connection.QueryAsync<PackageDto>(
            @"SELECT pac.Id,
                     CASE
                         WHEN pac.PackageTypeId = 1 THEN 'Data Listing'
                         WHEN pac.PackageTypeId = 2 THEN 'Data Collection'
                         WHEN pac.PackageTypeId = 3 THEN 'Enrollment'
                         WHEN pac.PackageTypeId = 4 THEN 'Digital Payment Enrollment'
                         WHEN pac.PackageTypeId = 5 THEN 'Community Validation'
                     END AS PackageType,
                     pac.OrgUnitId,
                     pac.ParentOrgUnitName,
                     pac.UniqueCode,
                     pac.[Description],
                     FORMAT(pac.Created, 'yyyy-MM-dd hh:mm tt') AS Created,
                     pac.FormId
              FROM [staging].[Package] pac
              WHERE pac.PackageTypeId IN (1, 2, 5)
                AND pac.PackageClosed_Status = 0
                AND EXISTS (
                    SELECT dev.Id
                    FROM [staging].[PackageEvent] evt
                    INNER JOIN [staging].[PackageEventDevice] dev ON dev.PackageEventId = evt.Id
                    WHERE evt.PackageId = pac.Id AND dev.DeviceId = @DeviceId
                )",
            new { DeviceId = deviceId });

        var packageList = packages.AsList();
        if (packageList.Count == 0) return packageList;

        var packageIds = packageList.Select(p => p.Id).ToList();

        var eventRows = await connection.QueryAsync<PackageEventRow>(
            @"SELECT pevt.PackageId AS OwnerId,
                     pevt.Id,
                     pevt.OrgUnitId,
                     pevt.OrgUnitName,
                     CASE
                         WHEN pevt.PackageStatusId = 1 THEN 'Stage Preparation'
                         WHEN pevt.PackageStatusId = 2 THEN 'Data Management'
                         WHEN pevt.PackageStatusId = 3 THEN 'Data Acceptance'
                         WHEN pevt.PackageStatusId = 4 THEN 'Gateway'
                     END AS PackageStatus,
                     CASE
                         WHEN pevt.PackageSubStatusId = 1 THEN 'Data Listing'
                         WHEN pevt.PackageSubStatusId = 2 THEN 'Data Collection'
                     END AS PackageSubStatus,
                     COUNT(peh.Id) AS HouseholdCount
              FROM [staging].[PackageEvent] pevt
              LEFT JOIN [staging].[PackageEventHousehold] peh ON peh.PackageEventId = pevt.Id
              WHERE pevt.PackageId IN @PackageIds
              GROUP BY pevt.PackageId, pevt.Id, pevt.OrgUnitId, pevt.OrgUnitName, pevt.PackageStatusId, pevt.PackageSubStatusId
              ORDER BY pevt.Id",
            new { PackageIds = packageIds });

        var eventsByPackage = eventRows
            .GroupBy(e => e.OwnerId)
            .ToDictionary(g => g.Key, g => (IEnumerable<PackageEventDto>)g.Select(e => e.ToDto()).ToList());

        var villageRows = await connection.QueryAsync<VillageRow>(
            @"SELECT pevt.PackageId AS OwnerId, ou_v.[Name] AS Village
              FROM [staging].[PackageEvent] pevt
              INNER JOIN [nissa].[OrgUnit] ou_ea ON pevt.OrgUnitId = ou_ea.OrgUnitGuid
              INNER JOIN [nissa].[OrgUnit] ou_v  ON ou_v.ParentOrgUnitId = ou_ea.Id
              WHERE pevt.PackageId IN @PackageIds
              ORDER BY ou_v.[Name]",
            new { PackageIds = packageIds });

        var villagesByPackage = villageRows
            .GroupBy(v => v.OwnerId)
            .ToDictionary(g => g.Key, g => (IEnumerable<string>)g.Select(v => v.Village).ToList());

        var formIds = packageList
            .Where(p => p.FormId.HasValue)
            .Select(p => p.FormId!.Value)
            .Distinct()
            .ToList();

        var formCache = new Dictionary<int, FormDto>();
        foreach (var formId in formIds)
            formCache[formId] = await LoadFormAsync(formId, connection);

        foreach (var package in packageList)
        {
            package.Events   = eventsByPackage.TryGetValue(package.Id, out var evts)     ? evts     : [];
            package.Villages = villagesByPackage.TryGetValue(package.Id, out var villages) ? villages : [];
            if (package.FormId.HasValue && formCache.TryGetValue(package.FormId.Value, out var form))
                package.Forms.Add(form);
        }

        return packageList;
    }

    public async Task<IEnumerable<PackageEventForManagementDto>> GetPackageEventsForManagementAsync(int packageId, int packageStatusId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            const string sql = @"
                WITH HouseholdStats AS (
                    SELECT hh.PackageEventId,
                           COUNT(*)                                                        AS HouseholdCount,
                           SUM(CASE WHEN hh.ListingStatusId    = 2 THEN 1 ELSE 0 END)     AS HouseholdListedCount,
                           SUM(CASE WHEN hh.CollectionStatusId = 2 THEN 1 ELSE 0 END)     AS HouseholdEnumeratedCount,
                           SUM(CASE WHEN hh.Accepted_Status    = 1 THEN 1 ELSE 0 END)     AS HouseholdAcceptedCount,
                           SUM(CASE WHEN hh.Rejected_Status    = 1 THEN 1 ELSE 0 END)     AS HouseholdRejectedCount
                    FROM [staging].[PackageEventHousehold] hh
                    INNER JOIN [staging].[PackageEvent] pe ON pe.Id = hh.PackageEventId
                    WHERE pe.PackageId = @PackageId
                    GROUP BY hh.PackageEventId
                ),
                FlagStats AS (
                    SELECT flag.PackageEventId,
                           COUNT(*) AS NumberFlags
                    FROM [staging].[PackageEventDataFlag] flag
                    INNER JOIN [staging].[PackageEvent] pe ON pe.Id = flag.PackageEventId
                    WHERE pe.PackageId = @PackageId
                    GROUP BY flag.PackageEventId
                ),
                PayloadStats AS (
                    SELECT hh.PackageEventId,
                           COUNT(*) AS HouseholdPayloadCount
                    FROM [staging].[PackageEventHousehold] hh
                    INNER JOIN [staging].[PackageEvent] pe ON pe.Id = hh.PackageEventId
                    WHERE pe.PackageId = @PackageId
                      AND EXISTS (SELECT 1 FROM [staging].[PackageEventHouseholdSynch] s WHERE s.PackageEventHouseholdId = hh.Id)
                    GROUP BY hh.PackageEventId
                )
                SELECT pe.Id,
                       0           AS OrgUnitId,
                       pe.OrgUnitId AS OrgUnitGuid,
                       pe.OrgUnitName,
                       CASE
                           WHEN pe.PackageStatusId = 1 THEN 'Stage Preparation'
                           WHEN pe.PackageStatusId = 2 THEN 'Data Management'
                           WHEN pe.PackageStatusId = 3 THEN 'Data Acceptance'
                           WHEN pe.PackageStatusId = 4 THEN 'Gateway'
                           ELSE 'UNKNOWN'
                       END AS PackageStatus,
                       CASE
                           WHEN pe.PackageSubStatusId = 1 THEN 'Data Listing'
                           WHEN pe.PackageSubStatusId = 2 THEN 'Data Collection'
                           ELSE 'UNKNOWN'
                       END AS PackageSubStatus,
                       ISNULL(hs.HouseholdCount,           0) AS HouseholdCount,
                       ISNULL(hs.HouseholdListedCount,      0) AS HouseholdListedCount,
                       ISNULL(hs.HouseholdEnumeratedCount,  0) AS HouseholdEnumeratedCount,
                       ISNULL(hs.HouseholdAcceptedCount,    0) AS HouseholdAcceptedCount,
                       ISNULL(hs.HouseholdRejectedCount,    0) AS HouseholdRejectedCount,
                       ISNULL(fs.NumberFlags,               0) AS NumberFlags,
                       CASE WHEN ISNULL(hs.HouseholdCount, 0) > 0
                            THEN CAST(hs.HouseholdListedCount     * 100.0 / hs.HouseholdCount AS int)
                            ELSE 0
                       END AS ListedPercentage,
                       CASE WHEN ISNULL(hs.HouseholdCount, 0) > 0
                            THEN CAST(hs.HouseholdEnumeratedCount * 100.0 / hs.HouseholdCount AS int)
                            ELSE 0
                       END AS EnumeratedPercentage,
                       0   AS StatusPercentage,
                       ISNULL(ps.HouseholdPayloadCount,     0) AS HouseholdPayloadCount,
                       ''  AS LastEnumerationDetail
                FROM [staging].[PackageEvent] pe
                LEFT JOIN HouseholdStats hs ON hs.PackageEventId = pe.Id
                LEFT JOIN FlagStats      fs ON fs.PackageEventId = pe.Id
                LEFT JOIN PayloadStats   ps ON ps.PackageEventId = pe.Id
                WHERE pe.PackageId      = @PackageId
                  AND pe.PackageStatusId = @PackageStatusId";

            return await connection.QueryAsync<PackageEventForManagementDto>(sql, new { PackageId = packageId, PackageStatusId = packageStatusId });
        }
    }

    public async Task<PagedResult<PackageEventHouseholdForManagementDto>> GetPackageEventHouseholdsForManagementAsync(
        int packageEventId,
        PackageEventHouseholdFilter filter,
        PaginationRequest pagination,
        string? village = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var where = filter switch
        {
            PackageEventHouseholdFilter.Draft =>
                "AND hh.ListingStatusId = 1",

            PackageEventHouseholdFilter.Listed =>
                "AND hh.ListingStatusId = 2",

            PackageEventHouseholdFilter.Submitted =>
                "AND hh.ListingStatusId = 4",

            PackageEventHouseholdFilter.Removed =>
                "AND hh.ListingStatusId = 3",

            PackageEventHouseholdFilter.Listed_NotCollected =>
                "AND hh.ListingStatusId = 2 AND hh.CollectionStatusId = 1",

            PackageEventHouseholdFilter.Listed_Collected =>
                "AND hh.ListingStatusId = 2 AND hh.CollectionStatusId = 2",

            PackageEventHouseholdFilter.Listed_WithFlags =>
                """
                    AND hh.ListingStatusId = 2
                    AND EXISTS (
                        SELECT 1
                        FROM staging.PackageEventDataFlag flag
                        WHERE flag.PackageEventId = @PackageEventId
                          AND flag.HouseholdId = hh.HouseholdGuid
                    )
                """,

            PackageEventHouseholdFilter.Listed_Collected_Unassigned =>
                "AND hh.ListingStatusId = 2 AND hh.CollectionStatusId = 2 AND hh.Accepted_ChangeDate IS NULL AND hh.Rejected_ChangeDate IS NULL",

            PackageEventHouseholdFilter.Listed_Collected_Accepted =>
                "AND hh.ListingStatusId = 2 AND hh.CollectionStatusId = 2 AND hh.Accepted_Status = 1",

            PackageEventHouseholdFilter.Listed_Collected_Rejected =>
                "AND hh.ListingStatusId = 2 AND hh.CollectionStatusId = 2 AND hh.Rejected_Status = 1",

            _ =>
                "AND hh.ListingStatusId = 2"
        };

        var applyVillage = !string.IsNullOrWhiteSpace(village);

        var villageFilter = applyVillage
            ? "AND hh.VillageName LIKE @Village"
            : string.Empty;

        var villageParam = applyVillage ? $"%{village!.Trim()}%" : null;

        var sql = $@"
        WITH PagedIds AS (
            SELECT hh.Id
            FROM staging.PackageEventHousehold hh
            WHERE hh.PackageEventId = @PackageEventId
            {where}
            {villageFilter}
            ORDER BY hh.HouseholdId
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
        ),
        FlagCounts AS (
            SELECT flag.PackageEventHouseholdId,
                   COUNT(*) AS FlagCount
            FROM staging.PackageEventDataFlag flag
            WHERE flag.PackageEventId = @PackageEventId
              AND flag.PackageEventHouseholdId IN (SELECT Id FROM PagedIds)
            GROUP BY flag.PackageEventHouseholdId
        )
        SELECT hh.Id,
               hh.HouseholdId,
               hh.HouseholdGuid,
               hh.VillageName,
               '' AS CreatedDetail,
               '' AS UpdatedDetail,
               hh.CommunityClassification AS Original_CommunityClassification,
               hh.HouseholdHead           AS Original_HouseholdHead,
               hh.ContactNumber           AS Original_ContactNumber,
               hh.PhysicalAddress         AS Original_PhysicalAddress,
               ISNULL(fc.FlagCount, 0)    AS NumberFlags,
               CASE
                   WHEN hh.Accepted_Status = 1 THEN 'Accepted'
                   WHEN hh.Rejected_Status = 1 THEN 'Rejected'
                   ELSE 'Unassigned'
               END AS AcceptanceStatus
        FROM staging.PackageEventHousehold hh
        INNER JOIN PagedIds pid ON pid.Id = hh.Id
        LEFT JOIN FlagCounts fc  ON fc.PackageEventHouseholdId = hh.Id
        ORDER BY hh.HouseholdId;

        SELECT COUNT(1)
        FROM staging.PackageEventHousehold hh
        WHERE hh.PackageEventId = @PackageEventId
        {where}
        {villageFilter};
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            PackageEventId = packageEventId,
            Offset = pagination.Offset,
            PageSize = pagination.PageSize,
            Village = villageParam
        });

        var households = (await multi
            .ReadAsync<PackageEventHouseholdForManagementDto>())
            .ToList();

        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<PackageEventHouseholdForManagementDto>(
            households,
            totalCount,
            pagination.Page,
            pagination.PageSize);
    }

    public async Task<PagedResult<DataFlagForListDto>> GetDataFlagsAsync(
        int packageId,
        int? packageEventId, 
        int? packageEventHouseholdId,
        string? searchTerm,
        PaginationRequest pagination)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var whereClauses = new List<string>
        {
            "pe.PackageId = @PackageId"
        };

        if (packageEventId.HasValue && packageEventId.Value > 0)
            whereClauses.Add("flag.PackageEventId = @PackageEventId");

        if (packageEventHouseholdId.HasValue && packageEventHouseholdId.Value > 0)
            whereClauses.Add("flag.PackageEventHouseholdId = @PackageEventHouseholdId");

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add(@"
        (
            flag.Subject LIKE @Search
            OR flag.Requester_FullName LIKE @Search
            OR hh.VillageName LIKE @Search
            OR hh.HouseholdHead LIKE @Search
            OR EXISTS (
                SELECT 1
                FROM staging.PackageEventHouseholdAttribute attr
                WHERE attr.PackageEventHouseholdId = hh.Id
                  AND attr.Modified_Value LIKE @Search
            )
        )");
        }

        var whereSql = "WHERE " + string.Join(" AND ", whereClauses);

        var sql = $@"
        SELECT
	    flag.Id,
        pe.Id AS PackageEventId,
	    hh.VillageName AS Village,
	    hh.HouseholdId,
	    hh.HouseholdHead AS Original_HouseholdHead,
	    ISNULL(
		    (
			    SELECT TOP 1 attr.Modified_Value 
			    FROM staging.PackageEventHouseholdAttribute attr
			    WHERE attr.PackageEventHouseholdId = hh.Id
			    AND attr.AttributeKey = 'Household Head'
		    )
	    , 'NOT LISTED') AS New_HouseholdHead,
	    ISNULL(
		    (
			    SELECT TOP 1 attr.Modified_Value 
			    FROM staging.PackageEventHouseholdAttribute attr
			    WHERE attr.PackageEventHouseholdId = hh.Id
			    AND attr.AttributeKey = 'Contact Number'
		    )
	    , 'NOT LISTED') AS New_ContactNumber,
	    CASE 
		    WHEN flag.DataFlagTypeId = 1 THEN 'Data Quality Issue' 
		    ELSE 'UKNOWN'
	    END AS DataFlagType,
	    CASE 
		    WHEN flag.DataFlagSubTypeId = 1 THEN 'Custom' 
		    WHEN flag.DataFlagSubTypeId = 2 THEN 'PMT Correction Required' 
		    WHEN flag.DataFlagSubTypeId = 3 THEN 'PMT Screening Required' 
		    WHEN flag.DataFlagSubTypeId = 4 THEN 'NICR LookUp Failed' 
		    ELSE 'UKNOWN'
	    END AS DataFlagSubType,
	    [Subject],
	    CASE 
		    WHEN flag.FlagResolved_Status = 0 AND flag.FlagDeferred_Status = 0 THEN 'Open' 
		    WHEN flag.FlagResolved_Status = 1 THEN 'Resolved' 
		    WHEN flag.FlagDeferred_Status = 1 THEN 'Deferred' 
	    END AS 'Status',
	    Requester_FullName AS RequesterName,
	    FORMAT (flag.Created, 'yyyy-MM-dd hh:mm tt') AS Created,
	    FORMAT (flag.FlagResolved_ChangeDate, 'yyyy-MM-dd hh:mm tt') AS Resolved
        FROM staging.PackageEventDataFlag flag
        INNER JOIN staging.PackageEventHousehold hh ON flag.PackageEventHouseholdId = hh.Id
        INNER JOIN staging.PackageEvent pe ON hh.PackageEventId = pe.Id
        {whereSql} 
        ORDER BY flag.Created DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1)
        FROM staging.PackageEventDataFlag flag
        INNER JOIN staging.PackageEventHousehold hh ON flag.PackageEventHouseholdId = hh.Id
        INNER JOIN staging.PackageEvent pe ON hh.PackageEventId = pe.Id
        {whereSql};
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            PackageId = packageId,
            PackageEventId = packageEventId,
            PackageEventHouseholdId = packageEventHouseholdId,
            Search = $"%{searchTerm}%",
            Offset = pagination.Offset,
            PageSize = pagination.PageSize
        });

        var dataFlags = (await multi
            .ReadAsync<DataFlagForListDto>())
            .ToList();

        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<DataFlagForListDto>(
            dataFlags,
            totalCount,
            pagination.Page,
            pagination.PageSize);

    }

    public async Task<PagedResult<PackageEventHouseholdDto>> GetCollectionPackageHouseholdsAsync(int packageEventId, PaginationRequest pagination)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
            SELECT evth.Id,
                   evth.HouseholdId,
                   evth.HouseholdGuid,
                   evth.VillageName,
                   FORMAT(evth.Created,      'yyyy-MM-dd hh:mm tt') AS CreatedDetail,
                   FORMAT(evth.LastModified, 'yyyy-MM-dd hh:mm tt') AS UpdatedDetail,
                   evth.HouseholdHead,
                   evth.CommunityClassification,
                   '' AS PMTScore,
                   evth.ContactNumber,
                   evth.PhysicalAddress,
                   CASE
                       WHEN evth.ListingStatusId = 1 THEN 'No Status'
                       WHEN evth.ListingStatusId = 2 THEN 'Listing And CBC Synched'
                       WHEN evth.ListingStatusId = 3 THEN 'New Household Synched'
                       WHEN evth.ListingStatusId = 4 THEN 'Disolved Or Duplicate Household Synched'
                   END AS ListingStatus,
                   CASE
                       WHEN evth.CollectionStatusId = 1 THEN 'No Status'
                       WHEN evth.CollectionStatusId = 2 THEN 'Enumeration Synched'
                   END AS CollectionStatus
            FROM [staging].PackageEventHousehold evth
            INNER JOIN [staging].PackageEvent evt ON evth.PackageEventId = evt.Id
            WHERE evt.Id = @PackageEventId
            ORDER BY evth.Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(1)
            FROM [staging].PackageEventHousehold evth
            INNER JOIN [staging].PackageEvent evt ON evth.PackageEventId = evt.Id
            WHERE evt.Id = @PackageEventId;";

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            PackageEventId = packageEventId,
            Offset         = pagination.Offset,
            PageSize       = pagination.PageSize
        });

        var households = (await multi.ReadAsync<PackageEventHouseholdDto>()).AsList();
        var totalCount = await multi.ReadSingleAsync<int>();

        await BatchPopulateAsync(households, connection);

        return new PagedResult<PackageEventHouseholdDto>(households, totalCount, pagination.Page, pagination.PageSize);
    }

    public async Task<PackageEventHouseholdIdDto> GetPackageEventHouseholdIdAsync(int packageEventId, int householdId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var sql = $@"
                    SELECT	TOP 1 
                            pac.Id,
		                    pac.PackageEventId,
		                    pac.HouseholdId
                    FROM [staging].[PackageEventHousehold] pac
                    WHERE pac.PackageEventId = {packageEventId}
	                    AND pac.HouseholdId = {householdId}
                    ORDER BY pac.Id asc";

            var packageEventHouseholdId = await connection.QuerySingleAsync<PackageEventHouseholdIdDto>(sql);

            return packageEventHouseholdId;
        }
    }

    public async Task<PagedResult<PackageEventHouseholdDto>> GetCommunityValidationPackageHouseholdsAsync(int packageEventId, PaginationRequest pagination)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
            SELECT evth.Id,
                   evth.HouseholdId,
                   evth.HouseholdGuid,
                   evth.VillageName,
                   FORMAT(evth.Created,      'yyyy-MM-dd hh:mm tt') AS CreatedDetail,
                   FORMAT(evth.LastModified, 'yyyy-MM-dd hh:mm tt') AS UpdatedDetail,
                   evth.HouseholdHead,
                   evth.CommunityClassification,
                   '' AS PMTScore,
                   evth.ContactNumber,
                   evth.PhysicalAddress,
                   CASE
                       WHEN evth.ListingStatusId = 1 THEN 'No Status'
                       WHEN evth.ListingStatusId = 2 THEN 'Listing And CBC Synched'
                       WHEN evth.ListingStatusId = 3 THEN 'New Household Synched'
                       WHEN evth.ListingStatusId = 4 THEN 'Disolved Or Duplicate Household Synched'
                   END AS ListingStatus,
                   CASE
                       WHEN evth.CollectionStatusId = 1 THEN 'No Status'
                       WHEN evth.CollectionStatusId = 2 THEN 'Enumeration Synched'
                   END AS CollectionStatus
            FROM [staging].PackageEventHousehold evth
            INNER JOIN [staging].PackageEvent evt ON evth.PackageEventId = evt.Id
            WHERE evt.Id = @PackageEventId
            ORDER BY evth.Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(1)
            FROM [staging].PackageEventHousehold evth
            INNER JOIN [staging].PackageEvent evt ON evth.PackageEventId = evt.Id
            WHERE evt.Id = @PackageEventId;";

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            PackageEventId = packageEventId,
            Offset         = pagination.Offset,
            PageSize       = pagination.PageSize
        });

        var households = (await multi.ReadAsync<PackageEventHouseholdDto>()).AsList();
        var totalCount = await multi.ReadSingleAsync<int>();

        await BatchPopulateAsync(households, connection);

        return new PagedResult<PackageEventHouseholdDto>(households, totalCount, pagination.Page, pagination.PageSize);
    }

    private async Task<FormDto> LoadFormAsync(int formId, SqlConnection connection)
    {
        var form = await connection.QuerySingleAsync<FormDto>(
            @"SELECT f.Id, f.ShortName, f.FriendlyName,
                     CONCAT(f.CurrentVersion_Major, '.', f.CurrentVersion_Minor) AS CurrentVersion
              FROM [staging].[Form] f WHERE f.Id = @FormId",
            new { FormId = formId });

        var attributeRows = await connection.QueryAsync<FormAttributeRow>(
            @"SELECT cat.ExtendableTypeName,
                     cat.ShortName AS Category,
                     ele.Id,
                     ele.CustomAttributeConfigurationGuid,
                     ele.AttributeKey,
                     CASE
                         WHEN ele.CustomAttributeTypeId = 1  THEN 'None'
                         WHEN ele.CustomAttributeTypeId = 2  THEN 'Numeric'
                         WHEN ele.CustomAttributeTypeId = 3  THEN 'Alpha Numeric'
                         WHEN ele.CustomAttributeTypeId = 4  THEN 'Selection'
                         WHEN ele.CustomAttributeTypeId = 5  THEN 'DateTime'
                         WHEN ele.CustomAttributeTypeId = 6  THEN 'First Class Property'
                         WHEN ele.CustomAttributeTypeId = 7  THEN 'Calculation'
                         WHEN ele.CustomAttributeTypeId = 8  THEN 'Photo'
                         WHEN ele.CustomAttributeTypeId = 9  THEN 'Multiselection'
                         WHEN ele.CustomAttributeTypeId = 10 THEN 'GPSCoords'
                     END AS CustomAttributeType,
                     ele.AttributeCode,
                     ele.FriendlyName AS EnglishDescription,
                     ele.Help         AS EnglishHelp,
                     ''               AS SesothoDescription,
                     ''               AS SesothoHelp,
                     ele.IsRequired   AS Required,
                     ele.StringMaxLength,
                     ele.NumericMinValue,
                     ele.NumericMaxValue,
                     ele.FutureDateOnly,
                     ele.PastDateOnly,
                     ele.RegEx
              FROM [staging].[FormCategory] cat
              INNER JOIN [staging].[FormElement] ele ON cat.Id = ele.FormCategoryId
              WHERE cat.FormId = @FormId
                AND cat.ExtendableTypeName IN ('Household', 'HouseholdMember')
              ORDER BY cat.[Order], ele.[Order]",
            new { FormId = formId });

        var allRows = attributeRows.ToList();

        var selectionKeys = allRows
            .Where(r => r.CustomAttributeType == "Selection" || r.CustomAttributeType == "Multiselection")
            .Select(r => r.AttributeKey)
            .Distinct()
            .ToList();

        var selectionLookup = Enumerable.Empty<SelectionValueRow>().ToLookup(r => r.AttributeKey, r => r.ToDto());
        if (selectionKeys.Count > 0)
        {
            var selectionRows = await connection.QueryAsync<SelectionValueRow>(
                @"SELECT AttributeKey, SelectionKey AS [Key], [Value]
                  FROM [nissa].[SelectionDataItem]
                  WHERE AttributeKey IN @Keys
                  ORDER BY CAST(SelectionKey AS int) ASC",
                new { Keys = selectionKeys });

            selectionLookup = selectionRows.ToLookup(r => r.AttributeKey, r => r.ToDto());
        }

        foreach (var extendableTypeName in new[] { "Household", "HouseholdMember" })
        {
            var typeRows = allRows.Where(r => r.ExtendableTypeName == extendableTypeName).ToList();
            var categories = typeRows
                .GroupBy(r => r.Category)
                .Select(g => new FormCategoryDto
                {
                    Category = g.Key,
                    Attributes = g.Select(r => r.ToDto(selectionLookup[r.AttributeKey])).ToList()
                })
                .ToList();

            form.ExtendableTypes.Add(new FormExtendableTypeDto
            {
                ExtendableTypeName = extendableTypeName,
                Categories = categories
            });
        }

        return form;
    }

    private sealed class PackageEventRow
    {
        public int OwnerId { get; set; }
        public int Id { get; set; }
        public Guid OrgUnitId { get; set; }
        public string OrgUnitName { get; set; }
        public string PackageStatus { get; set; }
        public string PackageSubStatus { get; set; }
        public int HouseholdCount { get; set; }
        public PackageEventDto ToDto() => new()
        {
            Id = Id, OrgUnitId = OrgUnitId, OrgUnitName = OrgUnitName,
            PackageStatus = PackageStatus, PackageSubStatus = PackageSubStatus,
            HouseholdCount = HouseholdCount
        };
    }

    private sealed class VillageRow
    {
        public int OwnerId { get; set; }
        public string Village { get; set; }
    }

    private sealed class FormAttributeRow
    {
        public string ExtendableTypeName { get; set; }
        public string Category { get; set; }
        public int Id { get; set; }
        public Guid CustomAttributeConfigurationGuid { get; set; }
        public string AttributeKey { get; set; }
        public string CustomAttributeType { get; set; }
        public string AttributeCode { get; set; }
        public string EnglishDescription { get; set; }
        public string EnglishHelp { get; set; }
        public string SesothoDescription { get; set; }
        public string SesothoHelp { get; set; }
        public bool Required { get; set; }
        public int? StringMaxLength { get; set; }
        public int? NumericMinValue { get; set; }
        public int? NumericMaxValue { get; set; }
        public bool FutureDateOnly { get; set; }
        public bool PastDateOnly { get; set; }
        public string RegEx { get; set; }
        public FormAttributeDto ToDto(IEnumerable<FormAttributeSelectionValueDto> selectionValues) => new()
        {
            Id = Id,
            CustomAttributeConfigurationGuid = CustomAttributeConfigurationGuid,
            AttributeKey = AttributeKey,
            CustomAttributeType = CustomAttributeType,
            AttributeCode = AttributeCode,
            EnglishDescription = EnglishDescription,
            EnglishHelp = EnglishHelp,
            SesothoDescription = SesothoDescription,
            SesothoHelp = SesothoHelp,
            Required = Required,
            StringMaxLength = StringMaxLength,
            NumericMinValue = NumericMinValue,
            NumericMaxValue = NumericMaxValue,
            FutureDateOnly = FutureDateOnly,
            PastDateOnly = PastDateOnly,
            RegEx = RegEx,
            SelectionValues = selectionValues.ToList()
        };
    }

    private sealed class SelectionValueRow
    {
        public string AttributeKey { get; set; }
        public int Key { get; set; }
        public string Value { get; set; }
        public FormAttributeSelectionValueDto ToDto() => new() { Key = Key, Value = Value };
    }

    private static async Task<List<T>> QueryChunkedAsync<T>(
        SqlConnection connection, string sql, IList<int> ids, int chunkSize = 1000)
    {
        if (ids.Count == 0) return [];
        if (ids.Count <= chunkSize)
            return (await connection.QueryAsync<T>(sql, new { Ids = ids })).AsList();

        var results = new List<T>(ids.Count);
        foreach (var chunk in ids.Chunk(chunkSize))
            results.AddRange(await connection.QueryAsync<T>(sql, new { Ids = chunk }));
        return results;
    }

    private async Task BatchPopulateAsync(IList<PackageEventHouseholdDto> households, SqlConnection connection)
    {
        if (households.Count == 0) return;

        var householdIds = households.Select(h => h.Id).ToList();

        var householdAttrs = await QueryChunkedAsync<HouseholdAttrRow>(connection,
            @"SELECT attr.PackageEventHouseholdId AS OwnerId,
                     cus.Category,
                     attr.AttributeKey AS [Key],
                     '' AS Value,
                     0 AS PMT,
                     '' AS SelectionValue
              FROM [staging].[PackageEventHouseholdAttribute] attr
              INNER JOIN [nissa].[CustomAttributeConfiguration] cus
                  ON attr.AttributeKey = cus.AttributeKey AND cus.ExtendableTypeName = 'Household'
              WHERE attr.PackageEventHouseholdId IN @Ids
              ORDER BY cus.Id ASC",
            householdIds);

        var attrsByHousehold = householdAttrs
            .GroupBy(r => r.OwnerId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.ToDto()).ToList());

        var memberRows = await QueryChunkedAsync<MemberRow>(connection,
            @"SELECT evthm.Id,
                     evth.HouseholdId,
                     evth.HouseholdGuid,
                     evthm.HouseholdMemberId,
                     evthm.HouseholdMemberGuid,
                     evthm.FirstName AS Name,
                     evthm.Surname,
                     evthm.IDDocumentType,
                     evthm.IdentificationNumber,
                     evthm.DateOfBirth,
                     evthm.Gender,
                     FORMAT(evthm.Created, 'yyyy-MM-dd hh:mm tt') AS CreatedDetail,
                     FORMAT(evthm.LastModified, 'yyyy-MM-dd hh:mm tt') AS UpdatedDetail,
                     evth.CommunityClassification,
                     evthm.PackageEventHouseholdId AS OwnerId
              FROM [staging].PackageEventHouseholdMember evthm
              INNER JOIN [staging].PackageEventHousehold evth ON evthm.PackageEventHouseholdId = evth.Id
              WHERE evthm.PackageEventHouseholdId IN @Ids
              ORDER BY evthm.Id",
            householdIds);

        var memberAttrLookup = Enumerable.Empty<MemberAttrRow>().ToLookup(r => r.OwnerId, r => r.ToDto());
        if (memberRows.Count > 0)
        {
            var memberIds = memberRows.Select(m => m.Id).ToList();
            var memberAttrs = await QueryChunkedAsync<MemberAttrRow>(connection,
                @"SELECT attr.PackageEventHouseholdMemberId AS OwnerId,
                         cus.Category,
                         attr.AttributeKey AS [Key],
                         '' AS Value,
                         0 AS PMT,
                         '' AS SelectionValue
                  FROM [staging].[PackageEventHouseholdMemberAttribute] attr
                  INNER JOIN [nissa].[CustomAttributeConfiguration] cus
                      ON attr.AttributeKey = cus.AttributeKey AND cus.ExtendableTypeName = 'HouseholdMember'
                  WHERE attr.PackageEventHouseholdMemberId IN @Ids
                  ORDER BY cus.Id ASC",
                memberIds);

            memberAttrLookup = memberAttrs.ToLookup(r => r.OwnerId, r => r.ToDto());
        }

        var membersByHousehold = memberRows
            .GroupBy(m => m.OwnerId)
            .ToDictionary(g => g.Key, g => g.Select(m => m.ToDto(memberAttrLookup[m.Id])).ToList());

        foreach (var household in households)
        {
            household.HouseholdAttributes = attrsByHousehold.TryGetValue(household.Id, out var attrs)
                ? attrs
                : [];
            household.Members = membersByHousehold.TryGetValue(household.Id, out var members)
                ? members
                : [];
        }
    }

    private sealed class HouseholdAttrRow
    {
        public int OwnerId { get; set; }
        public string Category { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool PMT { get; set; }
        public string SelectionValue { get; set; }
        public AttributeValueDto ToDto() => new() { Category = Category, Key = Key, Value = Value, PMT = PMT, SelectionValue = SelectionValue };
    }

    private sealed class MemberAttrRow
    {
        public int OwnerId { get; set; }
        public string Category { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool PMT { get; set; }
        public string SelectionValue { get; set; }
        public AttributeValueDto ToDto() => new() { Category = Category, Key = Key, Value = Value, PMT = PMT, SelectionValue = SelectionValue };
    }

    private sealed class MemberRow
    {
        public int Id { get; set; }
        public int HouseholdId { get; set; }
        public Guid HouseholdGuid { get; set; }
        public int HouseholdMemberId { get; set; }
        public Guid HouseholdMemberGuid { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string IDDocumentType { get; set; }
        public string IdentificationNumber { get; set; }
        public string CreatedDetail { get; set; }
        public string UpdatedDetail { get; set; }
        public string CommunityClassification { get; set; }
        public int OwnerId { get; set; }

        public PackageEventHouseholdMemberDto ToDto(IEnumerable<AttributeValueDto> attrs) => new()
        {
            Id = Id,
            HouseholdId = HouseholdId,
            HouseholdGuid = HouseholdGuid,
            HouseholdMemberId = HouseholdMemberId,
            HouseholdMemberGuid = HouseholdMemberGuid,
            DateOfBirth = DateOfBirth,
            Gender = Gender,
            Name = Name,
            Surname = Surname,
            IDDocumentType = IDDocumentType,
            IdentificationNumber = IdentificationNumber,
            CreatedDetail = CreatedDetail,
            UpdatedDetail = UpdatedDetail,
            HouseholdMemberAttributes = attrs
        };
    }

    public async Task<DashboardFilterOptionsDto> GetDashboardFilterOptionsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
        SELECT DISTINCT ISNULL(pac.ParentOrgUnitName, 'Unknown') AS District
        FROM   staging.Package pac
        WHERE  pac.ParentOrgUnitName IS NOT NULL AND pac.ParentOrgUnitName != ''
        ORDER  BY District;

        SELECT pac.Id,
               ISNULL(pac.UniqueCode + ' – ', '') + ISNULL(pac.[Description], 'Package ' + CAST(pac.Id AS VARCHAR)) AS Label,
               ISNULL(pac.ParentOrgUnitName, 'Unknown') AS District
        FROM   staging.Package pac
        ORDER  BY pac.ParentOrgUnitName, pac.Id;
        ";

        using var multi = await connection.QueryMultipleAsync(sql);

        var districts = (await multi.ReadAsync<string>()).ToList();
        var packages  = (await multi.ReadAsync<DashboardPackageOptionDto>()).ToList();

        return new DashboardFilterOptionsDto
        {
            Districts = districts,
            Packages  = packages
        };
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(string? district = null, int? packageId = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
        -- 1. Overall household counts
        SELECT
            COUNT(*)                                                                                          AS TotalHouseholds,
            SUM(CASE WHEN hh.ListingStatusId  = 2 THEN 1 ELSE 0 END)                                        AS TotalListed,
            SUM(CASE WHEN hh.CollectionStatusId = 2 THEN 1 ELSE 0 END)                                      AS TotalCollected,
            SUM(CASE WHEN hh.ListingStatusId = 2 AND hh.CollectionStatusId = 1 THEN 1 ELSE 0 END)           AS TotalPending,
            SUM(CASE WHEN hh.CollectionStatusId = 2 AND hh.Accepted_Status  = 1 THEN 1 ELSE 0 END)          AS TotalAccepted,
            SUM(CASE WHEN hh.CollectionStatusId = 2 AND hh.Rejected_Status  = 1 THEN 1 ELSE 0 END)          AS TotalRejected,
            SUM(CASE WHEN hh.CollectionStatusId = 2
                          AND hh.Accepted_ChangeDate IS NULL
                          AND hh.Rejected_ChangeDate IS NULL THEN 1 ELSE 0 END)                              AS TotalUnassigned
        FROM   staging.PackageEventHousehold hh
        INNER  JOIN staging.PackageEvent pe  ON pe.Id  = hh.PackageEventId
        INNER  JOIN staging.Package      pac ON pac.Id = pe.PackageId
        WHERE  (@District  IS NULL OR pac.ParentOrgUnitName = @District)
          AND  (@PackageId IS NULL OR pe.PackageId          = @PackageId);

        -- 2. Total data flags
        SELECT COUNT(*) AS TotalFlags
        FROM   staging.PackageEventDataFlag flag
        INNER  JOIN staging.PackageEvent pe  ON pe.Id  = flag.PackageEventId
        INNER  JOIN staging.Package      pac ON pac.Id = pe.PackageId
        WHERE  (@District  IS NULL OR pac.ParentOrgUnitName = @District)
          AND  (@PackageId IS NULL OR pe.PackageId          = @PackageId);

        -- 3. Per-package-event progress
        SELECT
            pe.Id                                                                          AS PackageEventId,
            pe.OrgUnitName,
            CASE pe.PackageStatusId
                WHEN 1 THEN 'Stage Preparation'
                WHEN 2 THEN 'Data Management'
                WHEN 3 THEN 'Data Acceptance'
                WHEN 4 THEN 'Gateway'
                ELSE 'Unknown'
            END                                                                            AS PackageStatus,
            COUNT(hh.Id)                                                                   AS TotalHouseholds,
            SUM(CASE WHEN hh.ListingStatusId  = 2 THEN 1 ELSE 0 END)                      AS Listed,
            SUM(CASE WHEN hh.CollectionStatusId = 2 THEN 1 ELSE 0 END)                    AS Collected,
            SUM(CASE WHEN hh.Accepted_Status = 1    THEN 1 ELSE 0 END)                    AS Accepted,
            SUM(CASE WHEN hh.Rejected_Status = 1    THEN 1 ELSE 0 END)                    AS Rejected,
            ISNULL(f.FlagCount, 0)                                                         AS Flags
        FROM   staging.PackageEvent pe
        INNER  JOIN staging.Package pac ON pac.Id = pe.PackageId
        LEFT   JOIN staging.PackageEventHousehold hh ON hh.PackageEventId = pe.Id
        LEFT   JOIN (
            SELECT PackageEventId, COUNT(*) AS FlagCount
            FROM   staging.PackageEventDataFlag
            GROUP  BY PackageEventId
        ) f ON f.PackageEventId = pe.Id
        WHERE  (@District  IS NULL OR pac.ParentOrgUnitName = @District)
          AND  (@PackageId IS NULL OR pe.PackageId          = @PackageId)
        GROUP  BY pe.Id, pe.OrgUnitName, pe.PackageStatusId, f.FlagCount
        ORDER  BY pe.Id DESC;

        -- 4. Daily collection trend – last 14 days
        SELECT
            FORMAT(CAST(syn.PayloadProcessedDate AS DATE), 'yyyy-MM-dd') AS Date,
            COUNT(DISTINCT syn.PackageEventHouseholdId)                   AS Count
        FROM   staging.PackageEventHouseholdSynch syn
        INNER  JOIN staging.PackageEventHousehold hh ON hh.Id  = syn.PackageEventHouseholdId
        INNER  JOIN staging.PackageEvent          pe  ON pe.Id  = hh.PackageEventId
        INNER  JOIN staging.Package               pac ON pac.Id = pe.PackageId
        WHERE  syn.PayloadProcessedDate >= DATEADD(DAY, -13, CAST(GETDATE() AS DATE))
          AND  syn.PayloadProcessedId = 2
          AND  (@District  IS NULL OR pac.ParentOrgUnitName = @District)
          AND  (@PackageId IS NULL OR pe.PackageId          = @PackageId)
        GROUP  BY CAST(syn.PayloadProcessedDate AS DATE)
        ORDER  BY CAST(syn.PayloadProcessedDate AS DATE);

        -- 5. Top 10 flagged villages
        SELECT TOP 10
            hh.VillageName,
            COUNT(*) AS FlagCount
        FROM   staging.PackageEventDataFlag flag
        INNER  JOIN staging.PackageEventHousehold hh ON hh.Id  = flag.PackageEventHouseholdId
        INNER  JOIN staging.PackageEvent          pe  ON pe.Id  = hh.PackageEventId
        INNER  JOIN staging.Package               pac ON pac.Id = pe.PackageId
        WHERE  hh.VillageName IS NOT NULL AND hh.VillageName != ''
          AND  (@District  IS NULL OR pac.ParentOrgUnitName = @District)
          AND  (@PackageId IS NULL OR pe.PackageId          = @PackageId)
        GROUP  BY hh.VillageName
        ORDER  BY FlagCount DESC;
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new { District = district, PackageId = packageId });

        var counts    = await multi.ReadSingleAsync<dynamic>();
        var flagTotal = await multi.ReadSingleAsync<dynamic>();
        var progress  = (await multi.ReadAsync<PackageEventProgressDto>()).ToList();
        var trend     = (await multi.ReadAsync<DailyCollectionDto>()).ToList();
        var villages  = (await multi.ReadAsync<VillageFlagCountDto>()).ToList();

        return new DashboardStatsDto
        {
            TotalHouseholds      = (int)counts.TotalHouseholds,
            TotalListed          = (int)counts.TotalListed,
            TotalCollected       = (int)counts.TotalCollected,
            TotalPending         = (int)counts.TotalPending,
            TotalAccepted        = (int)counts.TotalAccepted,
            TotalRejected        = (int)counts.TotalRejected,
            TotalUnassigned      = (int)counts.TotalUnassigned,
            TotalFlags           = (int)flagTotal.TotalFlags,
            PackageEventProgress = progress,
            DailyCollectionTrend = trend,
            TopFlaggedVillages   = villages
        };
    }

    public async Task<IEnumerable<DashboardMapPointDto>> GetHouseholdMapPointsAsync(string? district = null, int? packageId = null)
    {
        var cacheKey = $"map-points|{district}|{packageId}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<DashboardMapPointDto> cached))
            return cached;

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
        ;WITH Parsed AS (
            SELECT
                COALESCE(peh.VillageName, '')   AS VillageName,
                COALESCE(peh.HouseholdHead, '') AS HouseholdHead,
                CASE WHEN peh.CollectionStatusId = 2 THEN 1 ELSE 0 END AS IsCollected,
                TRY_CAST(
                    SUBSTRING(
                        attr.Modified_Value,
                        CHARINDEX('latitude: ', attr.Modified_Value) + 10,
                        CHARINDEX(' - longitude', attr.Modified_Value)
                            - (CHARINDEX('latitude: ', attr.Modified_Value) + 10)
                    ) AS FLOAT
                ) AS Lat,
                TRY_CAST(
                    SUBSTRING(
                        attr.Modified_Value,
                        CHARINDEX('longitude: ', attr.Modified_Value) + 11,
                        LEN(attr.Modified_Value)
                    ) AS FLOAT
                ) AS Lng
            FROM staging.PackageEventHousehold peh
            INNER JOIN staging.PackageEvent  pe  ON pe.Id  = peh.PackageEventId
            INNER JOIN staging.Package       pac ON pac.Id = pe.PackageId
            INNER JOIN staging.PackageEventHouseholdAttribute attr
                ON attr.PackageEventHouseholdId = peh.Id
               AND attr.AttributeKey = 'GPS Coordinates'
               AND attr.Modified_Value IS NOT NULL
            WHERE (@District  IS NULL OR pac.ParentOrgUnitName = @District)
              AND (@PackageId IS NULL OR pe.PackageId          = @PackageId)
        )
        SELECT VillageName, HouseholdHead, IsCollected, Lat, Lng
        FROM Parsed
        WHERE Lat IS NOT NULL AND Lng IS NOT NULL";

        var rows = await connection.QueryAsync<HouseholdMapRow>(sql, new { District = district, PackageId = packageId });

        var points = rows.Select(r => new DashboardMapPointDto
        {
            Lat           = r.Lat,
            Lng           = r.Lng,
            IsCollected   = r.IsCollected,
            VillageName   = r.VillageName,
            HouseholdHead = r.HouseholdHead
        }).ToList();

        _cache.Set(cacheKey, points, TimeSpan.FromSeconds(90));
        return points;
    }

    private record HouseholdMapRow
    {
        public string VillageName   { get; init; }
        public string HouseholdHead { get; init; }
        public bool   IsCollected   { get; init; }
        public double Lat           { get; init; }
        public double Lng           { get; init; }
    }
}