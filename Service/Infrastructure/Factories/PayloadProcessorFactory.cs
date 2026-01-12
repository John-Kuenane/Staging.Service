using Staging.Domain.Factories;
using Staging.Domain.SeedWork;
using Staging.Domain.Services;
using System.Collections.Generic;

namespace Staging.Infrastructure.Factories;

public class PayloadProcessorFactory
    : IPayloadProcessorFactory
{
    private readonly Dictionary<Type, object> _processorMap;

    public PayloadProcessorFactory()
    {
        _processorMap = new Dictionary<Type, object>();
    }

    public void AddProcessor<T>(PayloadProcessor<T> processor) where T : Processor
    {
        _processorMap.Add(typeof(T), processor);
    }

    public async Task<ExecutionResult> ProcessAsync(Processor currentActivity, CancellationToken cancellationToken)
    {
        foreach (var item in _processorMap)
        {
            if (currentActivity.GetType().IsSubclassOf(item.Key) || currentActivity.GetType() == item.Key) // Cater for dynamic proxies which may be subclasses of the actual class.
            {
                return await ((IPayloadProcessor)item.Value).ExecuteAsync(currentActivity, cancellationToken);
            }
        }

        throw new NotSupportedException("Could not find a processor for activity of type: " + currentActivity.GetType());
    }
}
