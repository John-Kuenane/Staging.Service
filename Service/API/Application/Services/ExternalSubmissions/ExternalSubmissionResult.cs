namespace Staging.API.Application.Services.ExternalSubmissions;

public class ExternalSubmissionResult
{
    public bool Success { get; init; }
    public string? ExternalId { get; init; }
    public string? ResponsePayload { get; init; }
    public string? ErrorMessage { get; init; }
}
