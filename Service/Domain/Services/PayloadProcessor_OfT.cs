using Staging.Domain.SeedWork;

namespace Staging.Domain.Services;

public interface IPayloadProcessor
{
    Task<ExecutionResult> ExecuteAsync(Processor activity, CancellationToken cancellationToken);
}

public abstract class PayloadProcessor<T> 
	: IPayloadProcessor where T : Processor
{
	protected PayloadProcessor()
	{
	}

	public async Task<ExecutionResult> ExecuteAsync(T activity, CancellationToken cancellationToken)
	{
		var result = ExecuteCoreAsync(activity, cancellationToken);
		return await result;
	}

	protected abstract Task<ExecutionResult> ExecuteCoreAsync(T activity, CancellationToken cancellationToken);

    async Task<ExecutionResult> IPayloadProcessor.ExecuteAsync(Processor activity, CancellationToken cancellationToken)
	{
		var typedActivity = activity as T;

		if (typedActivity == null)
		{
			throw new InvalidOperationException("Cannot process this activity because it is not the correct activity type. Expected '" + typeof(T) + "' but was '" + activity.GetType() + "'.");
		}

		return await ExecuteAsync(typedActivity, cancellationToken);
	}
}
