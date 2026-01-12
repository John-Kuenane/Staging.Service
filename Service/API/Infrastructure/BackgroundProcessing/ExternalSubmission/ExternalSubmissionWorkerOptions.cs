namespace Staging.API.Infrastructure.BackgroundProcessing.ExternalSubmission;

public sealed class ExternalSubmissionWorkerOptions : WorkerOptions
{
    /// <summary>
    /// Maximum number of submission attempts before giving up.
    /// </summary>
    public int MaxAttempts { get; init; } = 5;

    /// <summary>
    /// Backoff period before retrying a failed submission.
    /// </summary>
    public int RetryAfterMinutes { get; init; } = 10;

    public TimeSpan RetryAfter => TimeSpan.FromMinutes(RetryAfterMinutes);
}