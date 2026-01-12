using Staging.Domain.SeedWork;
using Staging.Domain.Services;

namespace Staging.Domain.Factories;

public interface IPayloadProcessorFactory
{
    Task<ExecutionResult> ProcessAsync(Processor currentActivity, CancellationToken cancellationToken);
}
