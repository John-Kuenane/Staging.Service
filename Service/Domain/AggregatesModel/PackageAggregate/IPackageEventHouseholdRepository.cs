using System.Linq.Expressions;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

//This is just the RepositoryContracts or Interface defined at the Domain Layer

public interface IPackageEventHouseholdRepository : IRepository<PackageEventHousehold>
{
    Task<PackageEventHousehold> GetAsync(int packageEventHouseholdId);

    Task<List<int>> GetPendingSynchronisationIdsAsync(int batchSize, CancellationToken cancellationToken);

    Task<PackageEventHousehold?> GetBySynchronisationIdAsync(int synchronisationId);

    Task ResetProcessingToPendingAsync(CancellationToken cancellationToken);

    Task<PackageEventHousehold> GetAsync(Expression<Func<PackageEventHousehold, bool>> filter);

    Task<int> CountStuckSynchronisationsAsync(TimeSpan threshold, CancellationToken cancellationToken);

    void Update(PackageEventHousehold packageEventHousehold);
}
