using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.API.Application.Commands.PackageAggregate;

public class AddDataFlagCommandHandler : IRequestHandler<AddDataFlagCommand, CommandResponseDto>
{
    private readonly IPackageRepository _packageRepository;
    private readonly ILogger<AddDataFlagCommandHandler> _logger;

    public AddDataFlagCommandHandler(
        IPackageRepository packageRepository,
        ILogger<AddDataFlagCommandHandler> logger)
    {
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CommandResponseDto> Handle(AddDataFlagCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("----- Saving payload command : {@Command}", command);

        var packageToUpdate = await _packageRepository.GetAsync(command.PackageId);
        if (packageToUpdate == null)
        {
            return new CommandResponseDto() { Status = false, StatusMessage = "Unable to locate package", RecordId = int.MinValue };
        }

        var personIdentifier = new PersonIdentifier(
            fullName: command.RequesterName,
            email: command.RequesterEmail);

        var newDataFlag = packageToUpdate.AddDataFlag(
            packageEventId: command.PackageEventId,
            packageEventHouseholdId: command.PackageEventHouseholdId,
            requester: personIdentifier,
            dataFlagType: DataFlagType.From(command.DataFlagTypeId),
            dataFlagSubType: command.DataFlagSubTypeId.HasValue ? DataFlagSubType.From(command.DataFlagSubTypeId.Value) : null,
            subject: command.Subject,
            description: command.Description,
            issueDate: command.IssueDate,
            priority: Priority.From(command.PriorityId),
            group: command.GroupId.HasValue ? Group.From(command.GroupId.Value) : null, 
            false);

        try
        {
            _packageRepository.Update(packageToUpdate);
            await _packageRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            return new CommandResponseDto() { Status = true, StatusMessage = "", RecordId = newDataFlag.Id };
        }
        catch (Exception ex)
        {
            _logger.LogError("----- Error processing payloads command : {@Command} : {@Message}", command, ex);

            return new CommandResponseDto() { Status = false, StatusMessage = ex.Message, RecordId = int.MinValue };
        }
    }
}
