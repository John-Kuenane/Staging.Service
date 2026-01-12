namespace Staging.API.Infrastructure.BackgroundProcessing;

public interface IWorkerOptions
{
    int WorkerCount { get; }
    int StartupBatchSize { get; }
}

