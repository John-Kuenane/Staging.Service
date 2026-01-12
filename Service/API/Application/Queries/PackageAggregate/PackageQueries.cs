using Microsoft.Data.SqlClient;
using Staging.API.Application.Common.Filters;
using Staging.API.Application.Common.Pagination;
using Staging.API.Application.Dtos;
using Staging.API.Application.Models.Pagination;

namespace Staging.API.Application.Queries.PackageAggregate;

public class PackageQueries
    : IPackageQueries
{
    private string _connectionString = string.Empty;

    public PackageQueries(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
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
                var form = await PreparePackageFormAsync(package.FormId.Value, connection);
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

    public async Task<IEnumerable<PackageForManagementDto>> GetPackagesForManagementAsync()
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
			                (SELECT COUNT(*) FROM staging.PackageEvent evt INNER JOIN staging.PackageEventHousehold hh ON evt.Id = hh.PackageEventId WHERE evt.PackageId = pac.Id) AS NumberHouseholds,
			                (SELECT COUNT(*) FROM staging.PackageEvent evt INNER JOIN staging.PackageEventHousehold hh ON evt.Id = hh.PackageEventId WHERE evt.PackageId = pac.Id AND hh.CollectionStatusId = 2) AS NumberEnumerations,
			                (SELECT COUNT(*) FROM staging.PackageEvent evt INNER JOIN staging.PackageEventDataFlag flag ON evt.Id = flag.PackageEventId WHERE evt.PackageId = pac.Id) AS NumberDataFlags,
			                0 AS NumberCompletedSubPackages,
                            pac.FormId
					FROM [staging].[Package] pac";

            return await connection.QueryAsync<PackageForManagementDto>(sql);
        }
    }

    public async Task<IEnumerable<PackageDto>> GetCollectionPackagesForDeviceAsync(string deviceId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var packages = await connection.QueryAsync<PackageDto>(
                $@"
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
                            pac.FormId
					FROM [staging].[Package] pac
					WHERE pac.PackageTypeId IN (1, 2, 5)
                        AND pac.PackageClosed_Status = 0
						AND EXISTS 
							(
								SELECT dev.Id 
								FROM [staging].[PackageEvent] evt 
									INNER JOIN [staging].[PackageEventDevice] dev ON dev.PackageEventId = evt.Id
								WHERE evt.PackageId = pac.Id AND dev.DeviceId = '{deviceId}'
							)");

            foreach (var package in packages)
            {
                await PopulatePackageEventsAsync(package, connection);
                await PopulatePackageVillagesAsync(package, connection);
                if(package.FormId.HasValue)
                {
                    var form = await PreparePackageFormAsync(package.FormId.Value, connection);
                    package.Forms.Add(form);
                }
            }

            return packages;
        }
    }

    public async Task<IEnumerable<PackageEventForManagementDto>> GetPackageEventsForManagementAsync(int packageId, int packageStatusId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var packageEvents = await connection.QueryAsync<PackageEventForManagementDto>(
                $@"
                    SELECT 
	                    Id,
	                    0 AS OrgUnitId,
	                    OrgUnitId AS orgunitGuid,
	                    OrgUnitName,
	                    CASE 
		                    WHEN PackageStatusId  = 1 THEN 'Stage Preparation'
		                    WHEN PackageStatusId  = 2 THEN 'Data Management'
		                    WHEN PackageStatusId  = 3 THEN 'Data Acceptance'
		                    WHEN PackageStatusId  = 4 THEN 'Gateway'
		                    ELSE 'UNKNOWN'
	                    END AS PackageStatus,
	                    CASE 
		                    WHEN PackageSubStatusId  = 1 THEN 'Data Listing'
		                    WHEN PackageSubStatusId  = 2 THEN 'Data Collection'
		                    ELSE 'UNKNOWN'
	                    END AS PackageSubStatus,
	                    (SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id) AS HouseholdCount,
	                    (SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id AND hh.ListingStatusId = 2) AS HouseholdListedCount,
	                    (SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id AND hh.CollectionStatusId = 2) AS HouseholdEnumeratedCount,
	                    (SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id AND hh.Accepted_Status = 1) AS HouseholdAcceptedCount,
	                    (SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id AND hh.Rejected_Status = 1) AS HouseholdRejectedCount,
	                    (SELECT COUNT(*) FROM staging.PackageEventDataFlag flag WHERE flag.PackageEventId = pe.Id) AS NumberFlags,
	                    CASE 
		                    WHEN (SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id) > 0 THEN
			                    CAST(((SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id AND hh.ListingStatusId = 2) * 100.0 / (SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id)) AS int)
		                    ELSE 0
	                    END AS ListedPercentage,
	                    CASE 
		                    WHEN (SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id) > 0 THEN
			                    CAST(((SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id AND hh.CollectionStatusId = 2) * 100.0 / (SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id)) AS int)
		                    ELSE 0
	                    END AS EnumeratedPercentage,
	                    0.00 AS StatusPercentage,
	                    (SELECT COUNT(*) FROM staging.PackageEventHousehold hh WHERE hh.PackageEventId = pe.Id AND EXISTS(SELECT Id FROM staging.PackageEventHouseholdSynch synch WHERE synch.PackageEventHouseholdId = hh.Id)) AS HouseholdPayloadCount,
	                    '' AS LastEnumerationDetail
                    FROM staging.PackageEvent pe
                    WHERE PackageId = {packageId}
                    AND PackageStatusId = {packageStatusId}");

            foreach (var packageEvent in packageEvents)
            {
                //await PopulatePackageEventHouseholdAttributesAsync(household, connection);
                //await PopulatePackageEventHouseholdMembersAsync(household, connection);
            }

            return packageEvents;
        }
    }

    public async Task<PagedResult<PackageEventHouseholdForManagementDto>> GetPackageEventHouseholdsForManagementAsync(
        int packageEventId,
        PackageEventHouseholdFilter filter,
        PaginationRequest pagination)
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

        var sql = $@"
        SELECT 
            Id,
            HouseholdId,
            HouseholdGuid,
            VillageName,
            '' AS CreatedDetail,
            '' AS UpdatedDetail,
            CommunityClassification AS Original_CommunityClassification,
            HouseholdHead AS Original_HouseholdHead,
            ContactNumber AS Original_ContactNumber,
            PhysicalAddress AS Original_PhysicalAddress,
            (
                SELECT COUNT(*)
                FROM staging.PackageEventDataFlag flag
                WHERE flag.PackageEventId = @PackageEventId
                  AND flag.PackageEventHouseholdId = hh.Id
            ) AS NumberFlags,
            CASE 
                WHEN hh.Accepted_Status = 1 THEN 'Accepted'
                WHEN hh.Rejected_Status = 1 THEN 'Rejected'
                ELSE 'Unassigned'
            END AS AcceptanceStatus,
            ISNULL((
                SELECT TOP 1 Payload
                FROM staging.PackageEventHouseholdSynch synch
                WHERE synch.PackageEventHouseholdId = hh.Id
                ORDER BY synch.Created DESC
            ), '') AS LatestPayload
        FROM staging.PackageEventHousehold hh
        WHERE hh.PackageEventId = @PackageEventId
        {where}
        ORDER BY hh.HouseholdId
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1)
        FROM staging.PackageEventHousehold hh
        WHERE hh.PackageEventId = @PackageEventId
        {where};
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            PackageEventId = packageEventId,
            Offset = pagination.Offset,
            PageSize = pagination.PageSize
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

    public async Task<IEnumerable<PackageEventHouseholdDto>> GetCollectionPackageHouseholdsAsync(int packageEventId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var households = await connection.QueryAsync<PackageEventHouseholdDto>(
                $@"SELECT   evth.Id,
                            evth.HouseholdId,
		                    evth.HouseholdGuid,
		                    evth.VillageName,
		                    FORMAT (evth.Created, 'yyyy-MM-dd hh:mm tt') AS CreatedDetail,
		                    FORMAT (evth.LastModified, 'yyyy-MM-dd hh:mm tt') AS UpdatedDetail,
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
                    WHERE evt.Id = {packageEventId}");

            foreach (var household in households)
            {
                await PopulatePackageEventHouseholdAttributesAsync(household, connection);
                await PopulatePackageEventHouseholdMembersAsync(household, connection);
            }

            return households;
        }
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

    public async Task<IEnumerable<PackageEventHouseholdDto>> GetCommunityValidationPackageHouseholdsAsync(int packageEventId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var sql =
                $@"SELECT   evth.Id,
                            evth.HouseholdId,
		                    evth.HouseholdGuid,
		                    evth.VillageName,
		                    FORMAT (evth.Created, 'yyyy-MM-dd hh:mm tt') AS CreatedDetail,
		                    FORMAT (evth.LastModified, 'yyyy-MM-dd hh:mm tt') AS UpdatedDetail,
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
                    WHERE evt.Id = {packageEventId}";

            var households = await connection.QueryAsync<PackageEventHouseholdDto>(sql);

            foreach (var household in households)
            {
                await PopulatePackageEventHouseholdAttributesAsync(household, connection);
                await PopulatePackageEventHouseholdMembersAsync(household, connection);
            }

            return households;
        }
    }

    private async Task PopulatePackageEventsAsync(PackageDto package, SqlConnection connection)
    {
        package.Events = await connection.QueryAsync<PackageEventDto>(
            $@"SELECT	pevt.Id,
						pevt.OrgUnitId,
						pevt.OrgUnitName,
						CASE 
							WHEN pevt.PackageStatusId = 1 THEN 'Stage Preparation' 
							WHEN pevt.PackageStatusId = 2 THEN 'Data Management' 
							WHEN pevt.PackageStatusId = 3 THEN 'Data Acceptance' 
							WHEN pevt.PackageStatusId = 4 THEN 'Gateway' 
						END AS 'PackageStatus',
						CASE 
							WHEN pevt.PackageSubStatusId = 1 THEN 'Data Listing' 
							WHEN pevt.PackageStatusId = 2 THEN 'Data Collection' 
						END AS 'PackageSubStatus',		
						(SELECT COUNT(peh.Id) FROM [staging].[PackageEventHousehold] peh 
							WHERE peh.PackageEventId = pevt.Id
						) AS HouseholdCount
				FROM [staging].[PackageEvent] pevt
				WHERE	
					pevt.PackageId = {package.Id}");
    }

    private async Task PopulatePackageVillagesAsync(PackageDto package, SqlConnection connection)
    {
        package.Villages = await connection.QueryAsync<string>(
             $@"SELECT	ou_v.[Name]
				FROM [staging].[PackageEvent] pevt
					INNER JOIN [nissa].[OrgUnit] ou_ea ON pevt.OrgUnitId = ou_ea.OrgUnitGuid
					INNER JOIN [nissa].[OrgUnit] ou_v ON ou_v.ParentOrgUnitId = ou_ea.Id
				WHERE
					pevt.PackageId = {package.Id}
					ORDER BY ou_v.Name");
    }

    private async Task<FormDto> PreparePackageFormAsync(int formId, SqlConnection connection)
    {
        var sql =
            $@"SELECT   f.Id,
		                    f.ShortName,
		                    f.FriendlyName,
		                    CONCAT(f.CurrentVersion_Major, '.', f.CurrentVersion_Minor) AS CurrentVersion
                    FROM staging.Form f
                    WHERE f.Id = {formId}";

        var form = await connection.QuerySingleAsync<FormDto>(sql);

        var householdExtendableType = await ExtractFormAttributes(formId, "Household", connection);
        var householdMemberExtendableType = await ExtractFormAttributes(formId, "HouseholdMember", connection);

        form.ExtendableTypes.Add(householdExtendableType);
        form.ExtendableTypes.Add(householdMemberExtendableType);

        return form;
    }

    private async Task<FormExtendableTypeDto> ExtractFormAttributes(int formId, string extendableTypeName, SqlConnection connection)
    {
        var extendableType = new FormExtendableTypeDto()
        {
            ExtendableTypeName = extendableTypeName
        };

        extendableType.Categories = await connection.QueryAsync<FormCategoryDto>(
                $@"
                    SELECT ShortName AS Category
                    FROM [staging].[FormCategory] cat
                    WHERE FormId = {formId}
	                    AND ExtendableTypeName = '{extendableTypeName}'
                    ORDER BY [Order]");

        foreach (var category in extendableType.Categories)
        {
            category.Attributes = await connection.QueryAsync<FormAttributeDto>(
                $@"SELECT	ele.Id,
		                    CustomAttributeConfigurationGuid,
                            AttributeKey,
		                    CASE 
			                    WHEN CustomAttributeTypeId = 1 THEN 'None' 
			                    WHEN CustomAttributeTypeId = 2 THEN 'Numeric' 
			                    WHEN CustomAttributeTypeId = 3 THEN 'Alpha Numeric' 
			                    WHEN CustomAttributeTypeId = 4 THEN 'Selection' 
			                    WHEN CustomAttributeTypeId = 5 THEN 'DateTime'
			                    WHEN CustomAttributeTypeId = 6 THEN 'First Class Property' 
                                WHEN CustomAttributeTypeId = 7 THEN 'Calculation' 
                                WHEN CustomAttributeTypeId = 8 THEN 'Photo' 
                                WHEN CustomAttributeTypeId = 9 THEN 'Multiselection' 
                                WHEN CustomAttributeTypeId = 10 THEN 'GPSCoords' 
		                    END AS CustomAttributeType,
		                    AttributeCode,
		                    ele.FriendlyName AS EnglishDescription,
                            ele.Help AS EnglishHelp,
		                    '' AS SesothoDescription,
                            '' AS SesothoHelp,
		                    IsRequired AS 'Required',
                            StringMaxLength,
                            NumericMinValue,
                            NumericMaxValue,
                            FutureDateOnly,
                            PastDateOnly,
                            RegEx
                    FROM [staging].[FormCategory] cat
	                    INNER JOIN [staging].[FormElement] ele ON cat.Id = ele.FormCategoryId
                    WHERE cat.FormId = {formId}
	                    AND Category = '{category.Category}' 
                    ORDER BY ele.[Order]");

            foreach (var attribute in category.Attributes)
            {
                if (attribute.CustomAttributeType == "Selection" || attribute.CustomAttributeType == "Multiselection")
                {
                    attribute.SelectionValues = await connection.QueryAsync<FormAttributeSelectionValueDto>(
                        $@"SELECT   SelectionKey, [Value]
                        FROM [nissa].[SelectionDataItem]
                        WHERE AttributeKey = '{attribute.AttributeKey}'
                        ORDER BY CAST(SelectionKey as int) ASC");
                }
            }
        }

        return extendableType;
    }

    private async Task PopulatePackageEventHouseholdAttributesAsync(PackageEventHouseholdDto household, SqlConnection connection)
    {
        household.HouseholdAttributes = await connection.QueryAsync<AttributeValueDto>(
            $@"SELECT	cus.Category,
		                attr.AttributeKey AS 'Key',
		                '' AS 'Value',
		                0 AS PMT,
		                '' AS SelectionValue
                FROM [staging].[PackageEventHouseholdAttribute] attr
                INNER JOIN [nissa].[CustomAttributeConfiguration] cus ON attr.AttributeKey = cus.AttributeKey AND cus.ExtendableTypeName = 'Household'
                WHERE attr.PackageEventHouseholdId = {household.Id}
                ORDER BY cus.Id ASC");
    }

    private async Task PopulatePackageEventHouseholdMembersAsync(PackageEventHouseholdDto household, SqlConnection connection)
    {
        household.Members = await connection.QueryAsync<PackageEventHouseholdMemberDto>(
            $@"SELECT   evthm.Id,
		                evth.HouseholdId,
		                evth.HouseholdGuid,
                        evthm.HouseholdMemberId,
		                evthm.HouseholdMemberGuid,
		                evthm.FirstName AS 'Name',
		                evthm.Surname,
		                evthm.IDDocumentType,
		                evthm.IdentificationNumber,
                        evthm.DateOfBirth,
                        evthm.Gender,
		                FORMAT (evthm.Created, 'yyyy-MM-dd hh:mm tt') AS CreatedDetail,
		                FORMAT (evthm.LastModified, 'yyyy-MM-dd hh:mm tt') AS UpdatedDetail,
		                0, 0, 0,
		                evth.CommunityClassification
                FROM [staging].PackageEventHouseholdMember evthm
                INNER JOIN [staging].PackageEventHousehold evth ON evthm.PackageEventHouseholdId = evth.Id
                WHERE evthm.PackageEventHouseholdId = {household.Id}
                ORDER BY evthm.Id");

        foreach (var householdmember in household.Members)
        {
            await PopulatePackageEventHouseholdMemberAttributesAsync(householdmember, connection);
        }
    }

    private async Task PopulatePackageEventHouseholdMemberAttributesAsync(PackageEventHouseholdMemberDto householdMember, SqlConnection connection)
    {
        householdMember.HouseholdMemberAttributes = await connection.QueryAsync<AttributeValueDto>(
            $@"SELECT	cus.Category,
		                attr.AttributeKey AS 'Key',
		                '' AS 'Value',
		                0 AS PMT,
		                '' AS SelectionValue
                FROM [staging].[PackageEventHouseholdMemberAttribute] attr
                INNER JOIN [nissa].[CustomAttributeConfiguration] cus ON attr.AttributeKey = cus.AttributeKey AND cus.ExtendableTypeName = 'HouseholdMember'
                WHERE attr.PackageEventHouseholdMemberId = {householdMember.Id}
                ORDER BY cus.Id ASC");
    }
}