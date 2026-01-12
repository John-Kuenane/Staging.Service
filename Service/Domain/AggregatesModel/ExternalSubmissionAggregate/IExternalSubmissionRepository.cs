namespace Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;

//This is just the RepositoryContracts or Interface defined at the Domain Layer

public interface IExternalSubmissionRepository : IRepository<ExternalSubmissionAggregate>
{
    Task<ExternalSubmissionAggregate?> GetAsync(int id);

    Task<List<int>> GetPendingSubmissionIdsAsync(
        int batchSize,
        int maxAttempts,
        TimeSpan? retryAfter,
        CancellationToken cancellationToken);

    Task<ExternalSubmissionAggregate?> GetBySubmissionIdAsync(int submissionId, CancellationToken cancellationToken);

    Task ResetProcessingToPendingAsync(CancellationToken cancellationToken);

    Task<int> CountStuckSubmissionsAsync(TimeSpan threshold, CancellationToken cancellationToken);

    ExternalSubmissionAggregate Add(ExternalSubmissionAggregate aggregate);

    Task<bool> SubmissionExistsAsync(
        string sourceType,
        int sourceId,
        ExternalSubmissionType type,
        string vendor,
        string requestKey,
        CancellationToken cancellationToken);

    void Update(ExternalSubmissionAggregate aggregate);
}
