namespace Staging.API.Infrastructure.BackgroundProcessing;

public class WorkerMonitor
{
    private int _activeWorkers;

    public void WorkerStarted() => Interlocked.Increment(ref _activeWorkers);
    public void WorkerStopped() => Interlocked.Decrement(ref _activeWorkers);
    public int ActiveWorkers => Volatile.Read(ref _activeWorkers);
}