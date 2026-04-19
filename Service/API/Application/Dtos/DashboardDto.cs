namespace Staging.API.Application.Dtos;

public record DashboardFilterOptionsDto
{
    public IEnumerable<string> Districts { get; set; } = [];
    public IEnumerable<DashboardPackageOptionDto> Packages { get; set; } = [];
}

public record DashboardPackageOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; }
    public string District { get; set; }
}

public record DashboardStatsDto
{
    public int TotalHouseholds { get; set; }
    public int TotalListed { get; set; }
    public int TotalCollected { get; set; }
    public int TotalPending { get; set; }
    public int TotalFlags { get; set; }
    public int TotalAccepted { get; set; }
    public int TotalRejected { get; set; }
    public int TotalUnassigned { get; set; }
    public IEnumerable<PackageEventProgressDto> PackageEventProgress { get; set; } = [];
    public IEnumerable<DailyCollectionDto> DailyCollectionTrend { get; set; } = [];
    public IEnumerable<VillageFlagCountDto> TopFlaggedVillages { get; set; } = [];
}

public record PackageEventProgressDto
{
    public int PackageEventId { get; set; }
    public string OrgUnitName { get; set; }
    public string PackageStatus { get; set; }
    public int TotalHouseholds { get; set; }
    public int Listed { get; set; }
    public int Collected { get; set; }
    public int Accepted { get; set; }
    public int Rejected { get; set; }
    public int Flags { get; set; }
}

public record DailyCollectionDto
{
    public string Date { get; set; }
    public int Count { get; set; }
}

public record VillageFlagCountDto
{
    public string VillageName { get; set; }
    public int FlagCount { get; set; }
}

public record DashboardMapPointDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public bool IsCollected { get; set; }
    public string VillageName { get; set; }
    public string HouseholdHead { get; set; }
}
