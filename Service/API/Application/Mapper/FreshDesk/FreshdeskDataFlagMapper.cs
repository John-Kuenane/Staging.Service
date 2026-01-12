using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.API.Application.Mapper.FreshDesk;

public class FreshdeskDataFlagMapper : IVendorMapper<PackageEventDataFlag>
{
    private static readonly Dictionary<int, long> FreshdeskGroupIdMap = new()
    {
        { 1, 501000337095 }, // Field Managers
        { 2, 501000342838 }, // IT Support
        { 3, 501000337067 }, // MoGYSD
        { 4, 501000342837 }, // QA/QC
        { 5, 501000337066 }  // TDL
    };

    public object Map(PackageEventDataFlag flag)
    {
        var groupId = flag.GroupId.HasValue ? FreshdeskGroupIdMap[flag.GroupId.Value] : FreshdeskGroupIdMap[1];

        return new
        {
            name = flag.Requester.FullName,
            email = flag.Requester.Email,
            subject = flag.Subject,
            description = flag.Description,
            priority = flag.PriorityId,
            group_id = groupId,
            status = 2,
            custom_fields = new
            {
                cf_reference_number = flag.PackageEventHouseholdId
            }
        };
    }
}