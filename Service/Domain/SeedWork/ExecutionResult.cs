namespace Staging.Domain.SeedWork;

public struct ExecutionResult
{
    public static ExecutionResult Failed(string activityType, string failureMessage)
    {
        return new ExecutionResult(activityType, false, failureMessage);
    }

    public static ExecutionResult Success(string activityType, Dictionary<string, object> metaData = null)
    {
        return new ExecutionResult(activityType, true, metaData);
    }

    private readonly bool successful;
    private readonly string resultMessage;
    private readonly Dictionary<string, object> metaData;

    private ExecutionResult(string activityType, bool successful, Dictionary<string, object> metaData)
    {
        this.successful = successful;
        this.resultMessage = string.Format("{0} completed successfully.", activityType);
        this.metaData = metaData;
    }

    private ExecutionResult(string activityType, bool successful, string resultMessage)
    {
        this.successful = successful;
        this.resultMessage = string.Format("{0} failed: {1}", activityType, resultMessage);
        this.metaData = null;
    }

    public bool Successful { get { return successful; } }
    public string ResultMessage { get { return resultMessage; } }
    public Dictionary<string, object> MetaData { get { return metaData; } }
}
