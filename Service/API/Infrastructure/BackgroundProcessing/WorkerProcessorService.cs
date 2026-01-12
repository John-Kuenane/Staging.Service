namespace Staging.API.Infrastructure.BackgroundProcessing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

public sealed class WorkerProcessorService<TWorkItem, TOptions, TMonitor> : BackgroundService
    where TOptions : class, IWorkerOptions
    where TMonitor : WorkerMonitor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWorkQueue<TWorkItem> _queue;
    private readonly TMonitor _monitor;
    private readonly ILogger<WorkerProcessorService<TWorkItem, TOptions, TMonitor>> _logger;
    private readonly int _workerCount;

    public WorkerProcessorService(
        IServiceScopeFactory scopeFactory,
        IWorkQueue<TWorkItem> queue,
        IOptions<TOptions> options,
        TMonitor monitor,
        ILogger<WorkerProcessorService<TWorkItem, TOptions, TMonitor>> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _workerCount = Math.Max(1, options.Value.WorkerCount);
    }

    public override async Task StartAsync(CancellationToken ct)
    {
        // One place for “requeue pending at startup”
        // Resolve scoped processor inside a scope
        using (var scope = _scopeFactory.CreateScope())
        {
            var processor = scope.ServiceProvider.GetRequiredService<IWorkerProcessor<TWorkItem>>();
            await processor.SeedAsync(_queue, ct);
        }

        await base.StartAsync(ct);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var workers = Enumerable.Range(0, _workerCount)
            .Select(i => RunWorkerAsync(i, ct));

        await Task.WhenAll(workers);
    }

    private async Task RunWorkerAsync(int workerId, CancellationToken ct)
    {
        _monitor.WorkerStarted();
        _logger.LogInformation("{Worker} {WorkerId} started. Active workers: {Count}",
            typeof(TWorkItem).Name, workerId, _monitor.ActiveWorkers);

        try
        {
            await foreach (var item in _queue.ReadAllAsync(ct))
            {
                try
                {
                    // ✅ Scope-per-item: safest with EF DbContext / tracking
                    using var scope = _scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IWorkerProcessor<TWorkItem>>();

                    await processor.ProcessAsync(item, workerId, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "{Worker} {WorkerId} exception processing item {Item}",
                        typeof(TWorkItem).Name, workerId, item);
                }
            }
        }
        finally
        {
            _monitor.WorkerStopped();
            _logger.LogInformation("{Worker} {WorkerId} stopped. Active workers: {Count}",
                typeof(TWorkItem).Name, workerId, _monitor.ActiveWorkers);
        }
    }
}
