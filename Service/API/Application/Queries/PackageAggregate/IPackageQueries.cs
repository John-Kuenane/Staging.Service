using Staging.API.Application.Common.Filters;
using Staging.API.Application.Common.Pagination;
using Staging.API.Application.Dtos;
using Staging.API.Application.Models.Pagination;

namespace Staging.API.Application.Queries.PackageAggregate;

public interface IPackageQueries
{
    Task<DashboardFilterOptionsDto> GetDashboardFilterOptionsAsync();
    Task<DashboardStatsDto> GetDashboardStatsAsync(string? district = null, int? packageId = null);
    Task<IEnumerable<DashboardMapPointDto>> GetHouseholdMapPointsAsync(string? district = null, int? packageId = null);
    Task<PackageForManagementDto> GetPackageForManagementAsync(int packageId);

    Task<DataFlagForDetailDto> GetPackageEventDataFlagAsync(int packageEventDataFlagId);

    Task<PackageEventHouseholdSynchForManagementDto> GetLatestSynchForHouseholdAsync(
        int packageEventId,
        int packageEventHouseholdId);

    Task<IEnumerable<PackageForManagementDto>> GetPackagesForManagementAsync();

    Task<IEnumerable<PackageDto>> GetCollectionPackagesForDeviceAsync(string deviceId);

    Task<IEnumerable<PackageEventForManagementDto>> GetPackageEventsForManagementAsync(int packageId, int packageStatusId);

    Task<PagedResult<DataFlagForListDto>> GetDataFlagsAsync(
        int packageId,
        int? packageEventId,
        int? packageEventHouseholdId,
        string? searchTerm,
        PaginationRequest pagination);

    Task<PagedResult<PackageEventHouseholdForManagementDto>> GetPackageEventHouseholdsForManagementAsync(
        int packageEventId,
        PackageEventHouseholdFilter filter,
        PaginationRequest pagination,
        string? village = null);

    Task<PackageEventHouseholdIdDto> GetPackageEventHouseholdIdAsync(int packageEventId, int householdId);

    Task<PagedResult<PackageEventHouseholdDto>> GetCollectionPackageHouseholdsAsync(int packageEventId, PaginationRequest pagination);

    Task<PagedResult<PackageEventHouseholdDto>> GetCommunityValidationPackageHouseholdsAsync(int packageEventId, PaginationRequest pagination);
}
