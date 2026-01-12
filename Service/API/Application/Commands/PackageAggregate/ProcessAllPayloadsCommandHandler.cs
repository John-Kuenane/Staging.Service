using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.API.Application.Commands.PackageAggregate;

// Regular CommandHandler
public class ProcessAllPayloadsCommandHandler : IRequestHandler<ProcessAllPayloadsCommand, bool>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IPackageEventRepository _packageEventRepository;
    private readonly ILogger<ProcessAllPayloadsCommandHandler> _logger;

    public ProcessAllPayloadsCommandHandler(
        IPackageRepository packageRepository,
        IPackageEventRepository packageEventRepository,
        ILogger<ProcessAllPayloadsCommandHandler> logger)
    {
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageEventRepository));
        _packageEventRepository = packageEventRepository ?? throw new ArgumentNullException(nameof(packageEventRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(ProcessAllPayloadsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("----- Processing all payloads command : {@Command}", command);

        var packageToProcess = await _packageRepository.GetWithEventsAsync(command.PackageId);
        if (packageToProcess == null)
        {
            return false;
        }

        try
        {
            foreach (var packageEvent in packageToProcess.Events)
            {
                var packageEventToProcess = await _packageEventRepository.GetAsync(packageEvent.Id);
                if (packageEventToProcess != null)
                {
                    try
                    {
                        packageEventToProcess.ProcessPayloads();
                        await _packageEventRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("----- Error processing all payloads command : {@Command} : {@Message}", command, ex);
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("----- Error processing all payloads command : {@Command} : {@Message}", command, ex);

            return false;
        }
    }
}
