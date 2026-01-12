namespace Staging.API.Application.Commands.PackageAggregate;

public record AddDataFlagCommand(
    int PackageId,
    int PackageEventId,
    int PackageEventHouseholdId,
    string RequesterName,
    string RequesterEmail,
    int DataFlagTypeId,
    int? DataFlagSubTypeId,
    string Subject,
    string Description,
    DateTime? IssueDate,
    int PriorityId,
    int? GroupId) : IRequest<CommandResponseDto>;