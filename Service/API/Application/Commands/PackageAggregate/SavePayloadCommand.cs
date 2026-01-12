namespace Staging.API.Application.Commands.PackageAggregate;

public record SavePayloadCommand(
    int PackageId,
    int PackageEventId,
    int HouseholdId,
    string DeviceId,
    string Payload) : IRequest<CommandResponseDto>;