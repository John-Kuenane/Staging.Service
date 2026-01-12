namespace Staging.API.Application.Dtos;

public record ExternalSubmissionDto
{
    public int Id { get; set; }
    public string Vendor { get; set; }
    public string Payload { get; set; }
    public string Status { get; set; }
    public string ExternalId { get; set; }
    public string Message { get; set; }
    public string Created { get; set; }
    public string LastAttempt { get; set; }
}