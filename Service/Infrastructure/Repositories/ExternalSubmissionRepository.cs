using Microsoft.EntityFrameworkCore;
using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;
using System.Collections.Generic;

namespace Staging.Infrastructure.Repositories;

public class ExternalSubmissionRepository : IExternalSubmissionRepository
{
    private readonly DatabaseContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public ExternalSubmissionRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Loads an external submission aggregate by id, including its submissions.
    /// </summary>
    /// <remarks>
    /// Returns the aggregate root with its <c>Submissions</c> collection eagerly loaded so that worker processing
    /// can safely mutate submission state within the aggregate boundary.
    /// </remarks>
    public async Task<ExternalSubmissionAggregate?> GetAsync(int id)
    {
        return await _context.ExternalSubmissions
            .Include(e => e.Submissions)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// Adds a new external submission aggregate (and its initial submissions) to the DbContext.
    /// </summary>
    /// <remarks>
    /// The aggregate is not written to the database until <c>UnitOfWork.SaveEntitiesAsync(...)</c> is called.
    /// </remarks>
    public ExternalSubmissionAggregate Add(ExternalSubmissionAggregate aggregate)
    {
        return _context.ExternalSubmissions.Add(aggregate).Entity;
    }

    /// <summary>
    /// Marks an external submission aggregate for update.
    /// </summary>
    /// <remarks>
    /// Used after workers mutate submission state (e.g., Processing → Success/Failed, attempt counts,
    /// external id, messages). If the aggregate is already tracked, EF change tracking will handle updates.
    /// </remarks>
    public void Update(ExternalSubmissionAggregate aggregate)
    {
        _context.ExternalSubmissions.Update(aggregate);
    }

    /// <summary>
    /// Returns a batch of submission ids eligible for worker processing.
    /// </summary>
    /// <remarks>
    /// Used to seed/refill a channel-based work queue without loading aggregates. Filters out successful
    /// submissions, items currently in Processing, items over the max attempt limit, and (optionally)
    /// items within a retry/backoff window.
    /// </remarks>
    public async Task<List<int>> GetPendingSubmissionIdsAsync(
        int batchSize,
        int maxAttempts,
        TimeSpan? retryAfter,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var cutoff = retryAfter.HasValue ? now - retryAfter.Value : (DateTime?)null;

        return await _context.ExternalSubmissions
            .SelectMany(a => a.Submissions)
            .Where(s =>
                s.Status != "Success" &&
                s.Status != "Processing" &&
                s.AttemptCount < maxAttempts &&
                (cutoff == null || s.LastAttemptAt == null || s.LastAttemptAt <= cutoff))
            .OrderBy(s => s.CreatedAt)   // ✅ fixes Skip/Take warning deterministically
            .Select(s => s.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Loads the aggregate root that owns the specified submission id.
    /// </summary>
    /// <remarks>
    /// The worker queue carries a submission id; this method resolves that id back to the owning
    /// <see cref="ExternalSubmissionAggregate"/> and eagerly loads <c>Submissions</c> so the worker can update
    /// submission state within the aggregate boundary.
    /// </remarks>
    public async Task<ExternalSubmissionAggregate?> GetBySubmissionIdAsync(
        int submissionId,
        CancellationToken cancellationToken)
    {
        return await _context.Set<ExternalSubmissionAggregate>()
            .Include(a => a.Submissions)
            .FirstOrDefaultAsync(
                a => a.Submissions.Any(s => s.Id == submissionId),
                cancellationToken);
    }

    /// <summary>
    /// Resets submissions left in Processing back to Pending (crash/restart recovery).
    /// </summary>
    /// <remarks>
    /// Executed at service startup to ensure work is not lost if a worker crashes or the service restarts.
    /// </remarks>
    public async Task ResetProcessingToPendingAsync(CancellationToken cancellationToken)
    {
        await _context.ExternalSubmissions
            .SelectMany(a => a.Submissions)
            .Where(s => s.Status == "Processing")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Status, "Pending"),
                cancellationToken);
    }

    /// <summary>
    /// Counts external submissions stuck in Processing longer than the given threshold.
    /// </summary>
    public async Task<int> CountStuckSubmissionsAsync(TimeSpan threshold, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - threshold;

        return await _context.ExternalSubmissions
            .SelectMany(a => a.Submissions)
            .Where(s => s.Status == "Processing"
                        && (s.LastAttemptAt == null || s.LastAttemptAt <= cutoff))
            .CountAsync(cancellationToken);
    }

    public async Task<bool> SubmissionExistsAsync(
        string sourceType,
        int sourceId,
        ExternalSubmissionType type,
        string vendor,
        string requestKey,
        CancellationToken cancellationToken)
    {
        return await _context.Set<ExternalSubmissionAggregate>()
            .Where(a => a.SourceType == sourceType && a.SourceId == sourceId)
            .SelectMany(a => a.Submissions)
            .AnyAsync(s =>
                s.Type == type &&
                s.Vendor == vendor &&
                s.RequestKey == requestKey &&
                (s.Status == "Pending" || s.Status == "Processing" || s.Status == "Success" || s.Status == "Submitted"),
                cancellationToken);
    }
}
