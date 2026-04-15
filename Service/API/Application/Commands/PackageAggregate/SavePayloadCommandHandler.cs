using Staging.API.Application.Queries.PackageAggregate;
using Staging.Domain.AggregatesModel.PackageAggregate;
using Staging.Domain.Services;

namespace Staging.API.Application.Commands.PackageAggregate;

// Regular CommandHandler
public class SavePayloadCommandHandler : IRequestHandler<SavePayloadCommand, CommandResponseDto>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IPackageEventRepository _packageEventRepository;
    private readonly IPackageEventHouseholdRepository _packageEventHouseholdRepository;
    private readonly IPackageQueries _packageQueries;
    private readonly ILogger<SavePayloadCommandHandler> _logger;
    private readonly IHouseholdIdAllocator _householdIdAllocator;

    public SavePayloadCommandHandler(
        IPackageRepository packageRepository,
        IPackageEventRepository packageEventRepository,
        IPackageEventHouseholdRepository packageEventHouseholdRepository,
        IPackageQueries packageQueries,
        IHouseholdIdAllocator householdIdAllocator,
        ILogger<SavePayloadCommandHandler> logger)
    {
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _packageEventRepository = packageEventRepository ?? throw new ArgumentNullException(nameof(packageEventRepository));
        _packageEventHouseholdRepository = packageEventHouseholdRepository ?? throw new ArgumentNullException(nameof(packageEventHouseholdRepository));
        _packageQueries = packageQueries ?? throw new ArgumentNullException(nameof(packageQueries));
        _householdIdAllocator = householdIdAllocator ?? throw new ArgumentNullException(nameof(householdIdAllocator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CommandResponseDto> Handle(SavePayloadCommand command, CancellationToken cancellationToken)
    {
        //_logger.LogDebug("----- Saving payload command : {@Command}", command);
        _logger.LogInformation("----- Saving payload command : {@HouseholdId}", command.HouseholdId);

        var packageToUpdate = await _packageRepository.GetAsync(command.PackageId);
        if (packageToUpdate == null)
        {
            return new CommandResponseDto() { Status = false, StatusMessage = "Unable to locate package", RecordId = int.MinValue };
        }

        try
        {
            var packageEvent = await _packageEventRepository.GetWithNoChildrenAsync(command.PackageEventId);
            if (packageEvent == null)
            {
                var returnNewId = await HandleMissingPackageEventForNewHousehold(
                    packageToUpdate,
                    command.Payload,
                    cancellationToken);
                await _packageRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                return new CommandResponseDto() { Status = true, StatusMessage = "", RecordId = returnNewId };
            }
            else
            {
                PackageEventHousehold packageEventHousehold = null;
                if (command.HouseholdId == 0)
                {
                    var returnNewId = await HandleNewHouseholdForCollection(
                        command.Payload,
                        packageEvent,
                        cancellationToken);
                    await _packageRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                    return new CommandResponseDto() { Status = true, StatusMessage = "", RecordId = returnNewId };
                }

                var packageEventHouseholdId = await _packageQueries.GetPackageEventHouseholdIdAsync(command.PackageEventId, command.HouseholdId);
                if (packageEventHouseholdId == null)
                {
                    throw new KeyNotFoundException(nameof(packageEventHouseholdId));
                }

                packageEventHousehold = await _packageEventHouseholdRepository.GetAsync(packageEventHouseholdId.Id);
                var synch = packageEventHousehold.AddSynchronisation("DVC-001", command.Payload);

                await _packageEventHouseholdRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                return new CommandResponseDto() { Status = true, StatusMessage = "", RecordId = packageEventHousehold.HouseholdId };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("----- Error saving payload command : {@Command} : {@Message}", command, ex.Message);

            var statusMessage = ex.InnerException == null ? ex.Message : $"{ex.Message} - {ex.InnerException.Message}"; 
            return new CommandResponseDto() { Status = false, StatusMessage = statusMessage, RecordId = int.MinValue };
        }
    }

    private async Task<int> HandleMissingPackageEventForNewHousehold(
        Package package,
        string payload,
        CancellationToken cancellationToken)
    {
        var packageEvent = package.Events.FirstOrDefault();
        if (packageEvent == null)
        {
            throw new InvalidOperationException("Unable to locate package event for new listing household.");
        }

        var newHouseholdId = await _householdIdAllocator.GetNextHouseholdIdAsync(cancellationToken);

        var packageEventHousehold = packageEvent.AddNewListingHousehold(newHouseholdId, payload);
        packageEventHousehold.AddSynchronisation("DVC-001", payload);

        return packageEventHousehold.HouseholdId;
    }

    private async Task<int> HandleNewHouseholdForCollection(
        string payload,
        PackageEvent packageEvent,
        CancellationToken cancellationToken)
    {
        var newHouseholdId = await _householdIdAllocator.GetNextHouseholdIdAsync(cancellationToken);

        var newPackageEventHousehold = packageEvent.AddNewCollectionHousehold(newHouseholdId, payload);
        newPackageEventHousehold.AddSynchronisation("DVC-001", payload);

        return newPackageEventHousehold.HouseholdId;
    }
}
