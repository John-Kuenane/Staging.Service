namespace Staging.API.Application.Commands.PackageAggregate;

public record ProcessPayloadsCommand(
    int PackageEventId) : IRequest<bool>;