namespace Staging.API.Application.Commands.PackageAggregate;

public record ChangeEventStatusToAcceptedCommand(
    int PackageEventId) : IRequest<bool>;