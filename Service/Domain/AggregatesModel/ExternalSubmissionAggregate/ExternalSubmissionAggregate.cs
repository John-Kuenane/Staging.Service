namespace Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;

public class ExternalSubmissionAggregate
    : Entity, IAggregateRoot
{
    public string SourceType { get; private set; } = default!;
    public int? SourceId { get; private set; }

    private readonly List<ExternalSubmission> _submissions = new();
    public IReadOnlyCollection<ExternalSubmission> Submissions => _submissions;

    private ExternalSubmissionAggregate() { } // EF Core

    public ExternalSubmissionAggregate(string sourceType, int? sourceId = null)
    {
        SourceType = sourceType;
        SourceId = sourceId;
    }

    public ExternalSubmission AddSubmission(ExternalSubmissionType type, string vendor, string payload, string? requestKey = null)
    {
        var submission = new ExternalSubmission(type, vendor, payload, requestKey);
        _submissions.Add(submission);
        return submission;
    }

    public ExternalSubmission? GetSubmission(string vendor)
        => _submissions.SingleOrDefault(s => s.Vendor == vendor);

    public void MarkSuccess(ExternalSubmission submission, string? externalId, string? response)
        => submission.MarkSuccess(externalId, response);

    public void MarkFailure(ExternalSubmission submission, string reason, string? response)
        => submission.MarkFailure(reason, response);
}