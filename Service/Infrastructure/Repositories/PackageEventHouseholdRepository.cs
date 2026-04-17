using Staging.Domain.AggregatesModel.PackageAggregate;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Staging.Infrastructure.Repositories;

public class PackageEventHouseholdRepository
    : IPackageEventHouseholdRepository
{
    private readonly DatabaseContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public PackageEventHouseholdRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Loads a household aggregate by id, including members and synchronisation records.
    /// </summary>
    /// <remarks>
    /// Retrieves the <see cref="PackageEventHousehold"/> aggregate and eagerly loads its <c>Members</c> and
    /// <c>Synchs</c> collections. This is typically used when you need the household graph in-memory to apply
    /// domain behavior and persist changes within the aggregate boundary.
    /// </remarks>
    public async Task<PackageEventHousehold> GetAsync(int packageEventHouseholdId)
    {
        var packageEventHousehold = await _context.PackageEventHouseholds
            .Include("Members")
            .Include("Synchs")
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == packageEventHouseholdId);

        return packageEventHousehold;
    }

    /// <summary>
    /// Marks a household aggregate for update in the current unit of work.
    /// </summary>
    /// <remarks>
    /// Registers changes made to the <see cref="PackageEventHousehold"/> aggregate so they will be persisted on the
    /// next call to <c>UnitOfWork.SaveEntitiesAsync(...)</c> / <c>SaveChangesAsync(...)</c>.
    /// If the aggregate was loaded in the same DbContext scope, EF change tracking may already capture modifications.
    /// </remarks>
    public void Update(PackageEventHousehold packageEventHousehold)
    {
        _context.Entry(packageEventHousehold).State = EntityState.Modified;
    }

    /// <summary>
    /// Loads a household aggregate matching the provided filter, including members and synchronisation records.
    /// </summary>
    /// <remarks>
    /// Retrieves the first <see cref="PackageEventHousehold"/> that matches the supplied predicate and eagerly loads
    /// its <c>Members</c> and <c>Synchs</c> collections. This is useful for query-by-criteria scenarios where the
    /// household aggregate is required for subsequent domain mutations.
    /// </remarks>
    public async Task<PackageEventHousehold> GetAsync(Expression<Func<PackageEventHousehold, bool>> filter)
    {
        var packageEventHousehold = await _context.PackageEventHouseholds
            .Include("Members")
            .Include("Synchs")
            .FirstOrDefaultAsync(filter);
        return packageEventHousehold;
    }

    /// <summary>
    /// Returns a batch of pending synchronisation ids for worker queueing.
    /// </summary>
    /// <remarks>
    /// Selects synchronisation record ids currently in the Pending state so the background worker can enqueue them
    /// without loading full household aggregates. This supports efficient queue seeding/replenishment.
    /// </remarks>
    public async Task<List<int>> GetPendingSynchronisationIdsAsync(int batchSize, CancellationToken cancellationToken)
    {
        return await _context.Set<PackageEventHouseholdSynch>()
            .Where(s => s.PayloadProcessedId == PayloadProcessedStatus.Pending.Id)
            .Select(s => s.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Loads the household aggregate that owns the specified synchronisation id.
    /// </summary>
    /// <remarks>
    /// Resolves a queued synchronisation id back to its owning <see cref="PackageEventHousehold"/> aggregate. The query
    /// eagerly loads the household’s attributes, members (and their attributes), and synchronisation records so the worker
    /// can process the synchronisation and mutate the aggregate safely within the aggregate boundary. Returns <c>null</c>
    /// if no household contains the requested synchronisation.
    /// </remarks>
    public async Task<PackageEventHousehold?> GetBySynchronisationIdAsync(int synchronisationId)
    {
        return await _context.PackageEventHouseholds
            .AsSplitQuery() // avoids huge single-query joins with multiple collections
            .Include(h => h.Attributes)
            .Include(h => h.Members)
                .ThenInclude(m => m.Attributes)
            .Include(h => h.Synchs)
            .Where(h => h.Synchs.Any(s => s.Id == synchronisationId))
            .FirstOrDefaultAsync()
            .ConfigureAwait(false); // avoids deadlocks in some contexts
    }

    /// <summary>
    /// Resets synchronisation records left in Processing back to Pending (crash/restart recovery).
    /// </summary>
    /// <remarks>
    /// Called at service startup to requeue work that may have been abandoned due to a crash or restart. Any synchronisation
    /// record still marked Processing is transitioned back to Pending so it becomes eligible for worker pickup again.
    /// </remarks>
    public async Task ResetProcessingToPendingAsync(CancellationToken cancellationToken)
    {
        await _context.Set<PackageEventHouseholdSynch>()
            .Where(s => s.PayloadProcessedId == PayloadProcessedStatus.Processing.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(
                x => x.PayloadProcessedId,
                PayloadProcessedStatus.Pending.Id),
                cancellationToken);
    }

    /// <summary>
    /// Counts synchronisation records that appear stuck in Processing longer than the given threshold.
    /// </summary>
    /// <remarks>
    /// Used by health checks/monitoring to detect synchronisation work items that have remained in Processing beyond an
    /// acceptable duration. A non-zero result typically indicates worker failure, deadlocks, or data causing repeated stalls.
    /// </remarks>
    public async Task<int> CountStuckSynchronisationsAsync(TimeSpan threshold, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - threshold;

        return await _context.Set<PackageEventHouseholdSynch>()
            .Where(x => x.PayloadProcessedId == PayloadProcessedStatus.Processing.Id &&
                        x.PayloadStatusDate < cutoff)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Counts synchronisation records currently in Failed state.
    /// </summary>
    public async Task<int> CountFailedSynchronisationsAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<PackageEventHouseholdSynch>()
            .Where(x => x.PayloadProcessedId == PayloadProcessedStatus.Failed.Id)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Counts recent failed synchronisations within a given time window.
    /// </summary>
    public async Task<int> CountRecentFailedSynchronisationsAsync(
        TimeSpan window,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - window;

        return await _context.Set<PackageEventHouseholdSynch>()
            .Where(x => x.PayloadProcessedId == PayloadProcessedStatus.Failed.Id &&
                        x.PayloadStatusDate >= cutoff)
            .CountAsync(cancellationToken);
    }
}
