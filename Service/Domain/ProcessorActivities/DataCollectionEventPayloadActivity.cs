using Staging.Domain.AggregatesModel.PackageAggregate;
using Staging.Domain.SeedWork;

namespace Staging.Domain.ProcessorActivities;

public class DataCollectionEventPayloadActivity
    : Processor
{
	public DataCollectionEventPayloadActivity(PackageEventHousehold packageEventHousehold, PackageEventHouseholdSynch packageEventHouseholdSynch) 
	{
        PackageEventHousehold = packageEventHousehold;
        PackageEventHouseholdSynch = packageEventHouseholdSynch;

    }

    public virtual PackageEventHousehold PackageEventHousehold { get; set; }

    public virtual PackageEventHouseholdSynch PackageEventHouseholdSynch { get; set; }

    public override string ActivityType
    {
        get { return "Data Collection Payload Synchronisation"; }
    }
}
