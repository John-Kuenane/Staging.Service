using Staging.API.Application.Common.Filters;
using Staging.API.Application.Common.Pagination;
using Staging.API.Application.Dtos;
using Staging.API.Application.Models.Pagination;

namespace Staging.API.Application.Queries.PackageAggregate;

public interface IPackageQueries
{
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
        PaginationRequest pagination);

    Task<PackageEventHouseholdIdDto> GetPackageEventHouseholdIdAsync(int packageEventId, int householdId);

    Task<IEnumerable<PackageEventHouseholdDto>> GetCollectionPackageHouseholdsAsync(int packageEventId);

    Task<IEnumerable<PackageEventHouseholdDto>> GetCommunityValidationPackageHouseholdsAsync(int packageEventId);
}
