using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.API.Application.Commands.PackageAggregate;

// Regular CommandHandler
public class ChangeEventStatusToGatewayCommandHandler : IRequestHandler<ChangeEventStatusToGatewayCommand, bool>
{
    private readonly IPackageEventRepository _packageEventRepository;
    private readonly ILogger<ChangeEventStatusToGatewayCommandHandler> _logger;

    public ChangeEventStatusToGatewayCommandHandler(
        IPackageEventRepository packageEventRepository,
        ILogger<ChangeEventStatusToGatewayCommandHandler> logger)
    {
        _packageEventRepository = packageEventRepository ?? throw new ArgumentNullException(nameof(packageEventRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(ChangeEventStatusToGatewayCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("----- Processing payloads command : {@Command}", command);

        var packageEventToProcess = await _packageEventRepository.GetAsync(command.PackageEventId);
        if (packageEventToProcess == null)
        {
            return false;
        }

        try
        {
            packageEventToProcess.ChangeStatusToGateway();
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
