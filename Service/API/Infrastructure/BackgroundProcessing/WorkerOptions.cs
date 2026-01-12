namespace Staging.API.Infrastructure.BackgroundProcessing;

public abstract class WorkerOptions : IWorkerOptions
{
    public int WorkerCount { get; init; } = Environment.ProcessorCount;
    public int StartupBatchSize { get; init; } = 1000;
}

