using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageStatus
        : Enumeration
{
    public static PackageStatus StagePreparation = new PackageStatus(1, "Stage Preparation");
    public static PackageStatus DataManagement = new PackageStatus(2, "Data Management");
    public static PackageStatus DataAcceptance = new PackageStatus(3, "Data Acceptance");
    public static PackageStatus Gateway = new PackageStatus(4, "Gateway");
    
public PackageStatus(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<PackageStatus> List() =>
        new[] { StagePreparation, DataManagement, DataAcceptance, Gateway };

    public static PackageStatus FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Package Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static PackageStatus From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Package Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}