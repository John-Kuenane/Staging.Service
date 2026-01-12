namespace Staging.API.Infrastructure.BackgroundProcessing;

public interface IWorkQueue<T>
{
    ValueTask EnqueueAsync(T item, CancellationToken cancellationToken);
    IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken);
    int ApproximateCount { get; }
    int Capacity { get; }
}
