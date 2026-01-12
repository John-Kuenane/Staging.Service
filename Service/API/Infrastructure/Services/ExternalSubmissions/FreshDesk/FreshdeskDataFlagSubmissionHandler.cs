using Microsoft.Extensions.Options;
using RestSharp;
using Staging.API.Application.Services.ExternalSubmissions;
using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Staging.API.Infrastructure.Services.ExternalSubmissions.FreshDesk;

public class FreshdeskDataFlagSubmissionHandler
    : IExternalSubmissionHandler
{
    public ExternalSubmissionType SubmissionType
        => ExternalSubmissionType.FreshdeskDataFlag;

    private readonly FreshDeskSettings _settings;
    private readonly RestClient _client;
    private readonly ILogger<FreshdeskDataFlagSubmissionHandler> _logger;

    public FreshdeskDataFlagSubmissionHandler(
        ILogger<FreshdeskDataFlagSubmissionHandler> logger,
        IOptions<FreshDeskSettings> options)
    {
        _logger = logger;
        _settings = options.Value;

        _client = new RestClient(new RestClientOptions(_settings.EndPoint)
        {
            ThrowOnAnyError = false,
            Timeout = TimeSpan.FromSeconds(15)
        });
    }

    public async Task<ExternalSubmissionResult> HandleAsync(
        ExternalSubmission submission,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestBody =
                JsonSerializer.Deserialize<object>(submission.Payload)!;

            var request = new RestRequest("tickets", Method.Post)
                .AddHeader(
                    "Authorization",
                    $"Basic {Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{_settings.ApiKey}:X"))}")
                .AddJsonBody(requestBody);

            var response = await _client.ExecuteAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Created)
            {
                return new ExternalSubmissionResult
                {
                    Success = false,
                    ErrorMessage = response.Content
                };
            }

            var ticketId = JsonDocument.Parse(response.Content!)
                .RootElement.GetProperty("id")
                .GetInt64()
                .ToString();

            return new ExternalSubmissionResult
            {
                Success = true,
                ExternalId = ticketId,
                ResponsePayload = response.Content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Freshdesk submission failed for SubmissionId={SubmissionId}",
                submission.Id);

            return new ExternalSubmissionResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
