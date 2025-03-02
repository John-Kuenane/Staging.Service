using Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEvent
    : Entity
{
    public StatusChange EventOpened { get; private set; }

    public StatusChange DeviceRegisteredStageClosed { get; private set; }

    public StatusChange DataManagementStageClosed { get; private set; }

    public StatusChange DataAcceptanceStageClosed { get; private set; }

    public StatusChange GatewayStageClosed { get; private set; }

    public Guid OrgUnitId { get; private set; }

    private List<PackageEventDevice> _devices;
    public IEnumerable<PackageEventDevice> Devices => _devices.AsReadOnly();

    private List<PackageEventHousehold> _households;
    public IEnumerable<PackageEventHousehold> Households => _households.AsReadOnly();

    private List<PackageEventDataFlag> _flags;
    public IEnumerable<PackageEventDataFlag> Flags => _flags.AsReadOnly();

    protected PackageEvent()
    {
        _devices = new List<PackageEventDevice>();
        _households = new List<PackageEventHousehold>();
        _flags = new List<PackageEventDataFlag>();
    }

    public PackageEvent(StatusChange eventOpened, StatusChange deviceRegisteredStageClosed, StatusChange dataManagementStageClosed, StatusChange dataAcceptanceStageClosed, StatusChange gatewayStageClosed, Guid orgUnitId)
    {
        OrgUnitId = orgUnitId;

        EventOpened = new StatusChange(true);
        DeviceRegisteredStageClosed = new StatusChange(false);
        DataManagementStageClosed = new StatusChange(false);
        DataAcceptanceStageClosed = new StatusChange(false);
        GatewayStageClosed = new StatusChange(false);
    }
}