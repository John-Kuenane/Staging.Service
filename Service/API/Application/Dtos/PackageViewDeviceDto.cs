namespace Staging.API.Application.Dtos;

public record PackageDto
{
    public int Id { get; set; }
    public string PackageType { get; set; }
    public Guid OrgUnitId { get; set; }
    public string ParentOrgUnitName { get; set; }
    public string UniqueCode { get; set; }
    public string Description { get; set; }
    public string Created { get; set; }
    public int? FormId { get; set; }

    public IEnumerable<PackageEventDto> Events { get; set; }
    public IEnumerable<string> Villages { get; set; }
    public ICollection<FormDto> Forms { get; set; } = new List<FormDto>();
}

public record PackageEventDto
{
    public int Id { get; set; }
    public Guid OrgUnitId { get; set; }
    public string OrgUnitName { get; set; }
    public string PackageStatus { get; set; }
    public string PackageSubStatus { get; set; }
    public int HouseholdCount { get; set; }
}

public record PackageEventHouseholdDto
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public Guid HouseholdGuid { get; set; }
    public string VillageName { get; set; }
    public string CreatedDetail { get; set; }
    public string UpdatedDetail { get; set; }
    public string HouseholdHead { get; set; }
    public string CommunityClassification { get; set; }
    public string CommunityValidationType { get; set; }
    public string PMTScore { get; set; }
    public string ContactNumber { get; set; }
    public string PostalAddress { get; set; }
    public string ListingStatus { get; set; }
    public string CollectionStatus { get; set; }
    public IEnumerable<AttributeValueDto> HouseholdAttributes { get; set; }
    public IEnumerable<PackageEventHouseholdMemberDto> Members { get; set; }
}

public record PackageEventHouseholdMemberDto
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public Guid HouseholdGuid { get; set; }
    public int HouseholdMemberId { get; set; }
    public Guid HouseholdMemberGuid { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string IDDocumentType { get; set; }
    public string IdentificationNumber { get; set; }
    public string CreatedDetail { get; set; }
    public string UpdatedDetail { get; set; }
    public bool IsHead { get; set; }
    public bool IsPayee { get; set; }
    public bool IsCurrent { get; set; }
    public IEnumerable<AttributeValueDto> HouseholdMemberAttributes { get; set; }

}

public record AttributeValueDto
{
    public string Category { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public bool PMT { get; set; }
    public string SelectionValue { get; set; }
}

public record FormDto
{
    public int Id { get; set; }
    public string ShortName { get; set; }
    public string FriendlyName { get; set; }
    public string CurrentVersion { get; set; }
    public ICollection<FormExtendableTypeDto> ExtendableTypes { get; set; } = new List<FormExtendableTypeDto>();
}

public record FormExtendableTypeDto
{
    public string ExtendableTypeName { get; set; }
    public IEnumerable<FormCategoryDto> Categories { get; set; } = new List<FormCategoryDto>();
}

public record FormCategoryDto
{
    public string Category { get; set; }
    public IEnumerable<FormAttributeDto> Attributes { get; set; } = new List<FormAttributeDto>();
}

public record FormAttributeDto
{
    public int Id { get; set; }
    public Guid CustomAttributeConfigurationGuid { get; set; }
    public string AttributeKey { get; set; }
    public string CustomAttributeType { get; set; }
    public string AttributeCode { get; set; }
    public string EnglishDescription { get; set; }
    public string SesothoDescription { get; set; }
    public string EnglishHelp { get; set; }
    public string SesothoHelp { get; set; }
    public bool Required { get; set; }
    public bool AllowNull { get; set; }
    public int? StringMaxLength { get; set; }
    public int? NumericMinValue { get; set; }
    public int? NumericMaxValue { get; set; }
    public bool FutureDateOnly { get; set; }
    public bool PastDateOnly { get; set; }
    public string RegEx { get; set; }
    public IEnumerable<FormAttributeSelectionValueDto> SelectionValues { get; set; } = new List<FormAttributeSelectionValueDto>();
}

public record FormAttributeSelectionValueDto
{
    public int Key { get; set; }
    public string Value { get; set; }
}

public record SynchronisationDto
{
    public int Id { get; set; }
    public Guid OrgUnitId { get; set; }
    public string OrgUnitName { get; set; }
    public string PackageStatus { get; set; }
    public string PackageSubStatus { get; set; }
    public int HouseholdCount { get; set; }
}