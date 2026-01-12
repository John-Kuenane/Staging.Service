namespace Staging.API.Application.Common.Filters;

public enum PackageEventHouseholdFilter
{
    Default = 0,

    Draft = 1,
    Listed = 2,
    Submitted = 3,
    Removed = 4,

    Listed_NotCollected = 5,
    Listed_Collected = 6,

    Listed_WithFlags = 7,

    Listed_Collected_Unassigned = 8,
    Listed_Collected_Accepted = 9,
    Listed_Collected_Rejected = 10
}
