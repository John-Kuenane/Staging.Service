namespace Staging.Domain.Models;

public class DataCollectionEventDetail
{
    public int Id { get; set; }
    public string HouseholdGuid { get; set; }
    public string NewContactNumber { get; set; }
    public string NewHouseholdHead { get; set; }
    public string NewCommunityClassification { get; set; }
    public List<DataCollectionEventDetailAttribute> HouseholdAttributes { get; set; }
    public List<DataCollectionEventMember> Members { get; set; }
}

public class DataCollectionEventMember
{
    public List<DataCollectionEventDetailAttribute> HouseholdMemberAttributes { get; set; }
    public int Id { get; set; }
    public string HouseholdMemberGuid { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string DateOfBirth { get; set; }
    public string Gender { get; set; }
    public MemberStatusDetails StatusDetails { get; set; }
}

public class DataCollectionEventDetailAttribute
{
    public string Category { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string Pmt { get; set; }
    public string SelectionValue { get; set; }
    public string NewValue { get; set; }
    public string ValueState { get; set; }
}

public class MemberStatusDetails
{
    public string Status { get; set; }
    public string RemovedReason { get; set; }
}


