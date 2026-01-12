using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;

namespace Staging.API.Application.Services.ExternalSubmissions;

public interface IExternalSubmissionHandler
{
    ExternalSubmissionType SubmissionType { get; }

    Task<ExternalSubmissionResult> HandleAsync(
        ExternalSubmission submission,
        CancellationToken cancellationToken);
}