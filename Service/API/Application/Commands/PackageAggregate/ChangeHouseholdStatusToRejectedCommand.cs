namespace Staging.API.Application.Commands.PackageAggregate;

public record ChangeHouseholdStatusToRejectedCommand(
    int PackageEventId,
    int PackageEventHouseholdId) : IRequest<bool>;