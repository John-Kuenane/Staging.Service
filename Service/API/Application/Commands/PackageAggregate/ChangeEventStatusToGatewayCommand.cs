namespace Staging.API.Application.Commands.PackageAggregate;

public record ChangeEventStatusToGatewayCommand(
    int PackageEventId) : IRequest<bool>;