namespace Staging.Domain.AggregatesModel.PackageAggregate;

//This is just the RepositoryContracts or Interface defined at the Domain Layer

public interface IPackageEventRepository : IRepository<PackageEvent>
{
    void Update(PackageEvent packageEvent);

    Task<PackageEvent> GetWithNoChildrenAsync(int packageEventId);

    Task<PackageEvent> GetAsync(int packageEventId);
}
