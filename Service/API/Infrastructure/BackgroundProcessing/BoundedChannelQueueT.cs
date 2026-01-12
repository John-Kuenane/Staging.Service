using System.Threading.Channels;

namespace Staging.API.Infrastructure.BackgroundProcessing;

public sealed class BoundedChannelQueue<T> : IWorkQueue<T>
{
    private readonly Channel<T> _channel;
    private int _count;

    public int ApproximateCount => Volatile.Read(ref _count);
    public int Capacity { get; }

    public BoundedChannelQueue(int capacity)
    {
        Capacity = capacity;
        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            SingleReader = false,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public async ValueTask EnqueueAsync(T item, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(item, cancellationToken);
        Interlocked.Increment(ref _count);
    }

    public async IAsyncEnumerable<T> ReadAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await _channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (_channel.Reader.TryRead(out var item))
            {
                Interlocked.Decrement(ref _count);
                yield return item;
            }
        }
    }
}
