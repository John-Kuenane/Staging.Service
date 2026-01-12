namespace Staging.API.Application.Commands.PackageAggregate;

public record ProcessAllPayloadsCommand(
    int PackageId) : IRequest<bool>;