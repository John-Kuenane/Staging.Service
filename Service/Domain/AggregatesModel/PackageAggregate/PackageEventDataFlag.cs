namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventDataFlag
    : Entity
{
    public int PackageEventHouseholdId { get; private set; }

    public int DataFlagTypeId { get; private set; }
    public int? DataFlagSubTypeId { get; private set; }

    public bool SystemGenerated { get; private set; }

    public StatusChange FlagResolved { get; private set; }

    public StatusChange FlagDeferred { get; private set; }

    public PersonIdentifier Requester { get; private set; }

    public string Subject { get; private set; }
    public string Description { get; private set; }
    public DateTime? IssueDate { get; private set; }

    public int PriorityId { get; private set; }

    public int? GroupId { get; private set; }

    private List<PackageEventDataFlagComment> _comments;
    public IEnumerable<PackageEventDataFlagComment> Comments => _comments.AsReadOnly();

    protected PackageEventDataFlag()
    {
        _comments = new List<PackageEventDataFlagComment>();
    }

    public PackageEventDataFlag(
        int packageEventHouseholdId, 
        PersonIdentifier requester,
        DataFlagType dataFlagType, 
        DataFlagSubType? dataFlagSubType,
        string subject,
        string description,
        DateTime? issueDate,
        Priority priority,
        Group? group,
        bool systemGenerated)
    {
        _comments = new List<PackageEventDataFlagComment>();

        PackageEventHouseholdId = packageEventHouseholdId;
        Requester = requester;

        DataFlagTypeId = dataFlagType.Id;
        DataFlagSubTypeId = dataFlagSubType != null ? dataFlagSubType.Id : null;

        Subject = subject;
        Description = description;
        IssueDate = issueDate;

        PriorityId = priority.Id;

        GroupId = group != null ? group.Id : null;

        FlagResolved = new StatusChange(false);
        FlagDeferred = new StatusChange(false);
        
        SystemGenerated = systemGenerated;
    }
}