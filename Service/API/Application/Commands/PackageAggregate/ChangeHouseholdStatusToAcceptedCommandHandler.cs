using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.API.Application.Commands.PackageAggregate;

// Regular CommandHandler
public class ChangeHouseholdStatusToAcceptedCommandHandler : IRequestHandler<ChangeHouseholdStatusToAcceptedCommand, bool>
{
    private readonly IPackageEventRepository _packageEventRepository;
    private readonly ILogger<ChangeHouseholdStatusToAcceptedCommandHandler> _logger;

    public ChangeHouseholdStatusToAcceptedCommandHandler(
        IPackageEventRepository packageEventRepository,
        ILogger<ChangeHouseholdStatusToAcceptedCommandHandler> logger)
    {
        _packageEventRepository = packageEventRepository ?? throw new ArgumentNullException(nameof(packageEventRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(ChangeHouseholdStatusToAcceptedCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("----- Processing payloads command : {@Command}", command);

        var packageEventToProcess = await _packageEventRepository.GetAsync(command.PackageEventId);
        if (packageEventToProcess == null)
        {
            return false;
        }

        try
        {
            packageEventToProcess.ChangeHouseholdStatusToAccepted(command.PackageEventHouseholdId);
            await _packageEventRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("----- Error processing payloads command : {@Command} : {@Message}", command, ex);

            return false;
        }
    }
}
