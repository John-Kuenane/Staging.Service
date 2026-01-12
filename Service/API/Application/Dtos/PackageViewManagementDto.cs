namespace Staging.API.Application.Dtos;

public record PackageForManagementDto
{
    public int Id { get; set; }
    public string PackageType { get; set; }
    public Guid OrgUnitId { get; set; }
    public string ParentOrgUnitName { get; set; }
    public string UniqueCode { get; set; }
    public string Description { get; set; }
    public string Created { get; set; }
    public int NumberHouseholds { get; set; }
    public int NumberEnumerations { get; set; }
    public int NumberDataFlags { get; set; }
    public int NumberCompletedSubPackages { get; set; }
    public int? FormId { get; set; }

    public ICollection<FormDto> Forms { get; set; } = new List<FormDto>();
}

public record PackageEventForManagementDto
{
    public int Id { get; set; }
    public int OrgUnitId { get; set; }
    public Guid OrgUnitGuid { get; set; }
    public string OrgUnitName { get; set; }
    public string PackageStatus { get; set; }
    public string PackageSubStatus { get; set; }
    public int HouseholdCount { get; set; }
    public int HouseholdListedCount { get; set; }
    public int HouseholdEnumeratedCount { get; set; }
    public int HouseholdAcceptedCount { get; set; }
    public int HouseholdRejectedCount { get; set; }
    public int HouseholdProcessedCount { get; set; }
    public int ListedPercentage { get; set; }
    public int EnumeratedPercentage { get; set; }
    public int StatusPercentage { get; set; }
    public int HouseholdPayloadCount { get; set; }
    public string LastEnumerationDetail { get; set; }
}

public record PackageEventHouseholdForManagementDto
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public Guid HouseholdGuid { get; set; }
    public string VillageName { get; set; }
    public string CreatedDetail { get; set; }
    public string UpdatedDetail { get; set; }
    public string Original_CommunityClassification { get; set; }
    public string Original_HouseholdHead { get; set; }
    public string Original_ContactNumber { get; set; }
    public string Original_PhysicalAddress { get; set; }
    public int NumberFlags { get; set; }
    public string AcceptanceStatus { get; set; }
}

public record PackageEventHouseholdSynchForManagementDto
{
    public int Id { get; set; }
    public string DeviceId { get; set; }
    public string Created { get; set; }
    public string Payload { get; set; }
}

public record DataFlagForListDto
{
    public int Id { get; set; }
    public int PackageEventId { get; set; }
    public string District { get; set; }
    public string CommunityCouncil { get; set; }
    public string EnumerationArea { get; set; }
    public string Village { get; set; }
    public int HouseholdId { get; set; }
    public string Original_HouseholdHead { get; set; }
    public string New_HouseholdHead { get; set; }
    public string New_ContactNumber { get; set; }
    public string DataFlagType { get; set; }
    public string DataFlagSubType { get; set; }
    public string Subject { get; set; }
    public string Status { get; set; }
    public string RequesterName { get; set; }
    public string Created { get; set; }
    public string Resolved { get; set; }
}

public record DataFlagForDetailDto
{
    public int Id { get; set; }
    public string DataFlagType { get; set; }
    public string DataFlagSubType { get; set; }
    public string Subject { get; set; }
    public string Status { get; set; }
    public string RequesterName { get; set; }
    public string RequesterEmail { get; set; }
    public string Description { get; set; }
    public string Priority { get; set; }
    public string Group { get; set; }
    public string Created { get; set; }
    public string Resolved { get; set; }
}

public record PackageEventHouseholdIdDto
{
    public int Id { get; set; }
    public int PackageEventId { get; set; }
    public int HouseholdId { get; set; }
}
