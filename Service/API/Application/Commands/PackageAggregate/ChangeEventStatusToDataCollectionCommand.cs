namespace Staging.API.Application.Commands.PackageAggregate;

public record ChangeEventStatusToDataCollectionCommand(
    int PackageEventId) : IRequest<bool>;