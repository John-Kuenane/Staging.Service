namespace Staging.Domain.AggregatesModel.PackageAggregate;

//This is just the RepositoryContracts or Interface defined at the Domain Layer

public interface IPackageRepository : IRepository<Package>
{
    void Update(Package package);

    Task<Package> GetAsync(int packageId);

    Task<Package> GetWithEventsAsync(int packageId);
}
