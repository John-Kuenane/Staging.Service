namespace Staging.API.Infrastructure.BackgroundProcessing;

public interface IWorkerProcessor<TWorkItem>
{
    /// Called once at startup (before workers start pulling from queue)
    Task SeedAsync(IWorkQueue<TWorkItem> queue, CancellationToken ct);

    /// Called for each queued work item
    Task ProcessAsync(TWorkItem item, int workerId, CancellationToken ct);
}

