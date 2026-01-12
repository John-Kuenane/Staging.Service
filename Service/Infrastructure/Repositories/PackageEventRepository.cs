using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure.Repositories;

public class PackageEventRepository
    : IPackageEventRepository
{
    private readonly DatabaseContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public PackageEventRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PackageEvent> GetWithNoChildrenAsync(int packageEventId)
    {
        var packageEvent = await _context.PackageEvents
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == packageEventId);

        return packageEvent;
    }

    public async Task<PackageEvent> GetAsync(int packageEventId)
    {
        var packageEvent = await _context.PackageEvents
            .Include("Households.Synchs")
            .Include("Households.Attributes")
            .Include("Households.Members.Attributes")
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == packageEventId);

        return packageEvent;
    }

    public void Update(PackageEvent packageEvent)
    {
        _context.Entry(packageEvent).State = EntityState.Modified;
    }
}
