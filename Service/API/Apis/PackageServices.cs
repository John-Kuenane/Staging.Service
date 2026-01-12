using Staging.API.Application.Queries.ExternalSubmissionAggregate;
using Staging.API.Application.Queries.PackageAggregate;

public class PackageServices(
    IMediator mediator,
    IPackageQueries queries,
    IExternalSubmissionQueries submissionQueries,
    //IIdentityService identityService,
    ILogger<PackageServices> logger)
{
    public IMediator Mediator { get; set; } = mediator;
    public ILogger<PackageServices> Logger { get; } = logger;
    public IPackageQueries Queries { get; } = queries;
    public IExternalSubmissionQueries SubmissionQueries { get; } = submissionQueries;
    //public IIdentityService IdentityService { get; } = identityService;
}
