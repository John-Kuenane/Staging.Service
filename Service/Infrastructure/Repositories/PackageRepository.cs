using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure.Repositories;

public class PackageRepository
    : IPackageRepository
{
    private readonly DatabaseContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public PackageRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Package> GetAsync(int packageId)
    {
        var package = await _context.Packages
            .Include("Events.Households")
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == packageId);

        return package;
    }

    public async Task<Package> GetWithEventsAsync(int packageId)
    {
        var package = await _context.Packages
            .Include("Events")
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == packageId);

        return package;
    }

    public void Update(Package package)
    {
        _context.Entry(package).State = EntityState.Modified;
    }
}
