using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.API.Application.Commands.PackageAggregate;

// Regular CommandHandler
public class ChangeHouseholdStatusToRejectedCommandHandler : IRequestHandler<ChangeHouseholdStatusToRejectedCommand, bool>
{
    private readonly IPackageEventRepository _packageEventRepository;
    private readonly ILogger<ChangeHouseholdStatusToRejectedCommandHandler> _logger;

    public ChangeHouseholdStatusToRejectedCommandHandler(
        IPackageEventRepository packageEventRepository,
        ILogger<ChangeHouseholdStatusToRejectedCommandHandler> logger)
    {
        _packageEventRepository = packageEventRepository ?? throw new ArgumentNullException(nameof(packageEventRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(ChangeHouseholdStatusToRejectedCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("----- Processing payloads command : {@Command}", command);

        var packageEventToProcess = await _packageEventRepository.GetAsync(command.PackageEventId);
        if (packageEventToProcess == null)
        {
            return false;
        }

        try
        {
            packageEventToProcess.ChangeHouseholdStatusToRejected(command.PackageEventHouseholdId);
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
