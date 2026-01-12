using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public static class HealthApi
{
    public static RouteGroupBuilder MapHealthApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/health")
            .HasApiVersion(1.0);

        // Simple liveness probe (used by load balancers)
        api.MapGet("/ping", () => Results.Ok("pong"));

        // Readiness probe (is the app ready to serve traffic?)
        api.MapHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Background sync processing health
        api.MapHealthChecks("/sync", new HealthCheckOptions
        {
            Predicate = check =>
                check.Name.StartsWith("sync_", StringComparison.OrdinalIgnoreCase),
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        return api;
    }
}
