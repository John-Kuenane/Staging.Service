namespace Staging.API.Application.Commands.PackageAggregate;

public record ChangeHouseholdStatusToAcceptedCommand(
    int PackageEventId,
    int PackageEventHouseholdId) : IRequest<bool>;