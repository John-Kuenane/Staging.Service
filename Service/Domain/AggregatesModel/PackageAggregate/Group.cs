using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class Group
        : Enumeration
{
    public static Group FieldManager = new Group(1, "Field Managers");
    public static Group ITSupport = new Group(2, "IT Support");
    public static Group MoGYSD = new Group(3, "MoGYSD");
    public static Group QAQC = new Group(4, "QA/QC");
    public static Group Vendor = new Group(5, "Vendor");

    public Group(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<Group> List() =>
        new[] { FieldManager, ITSupport, MoGYSD, QAQC, Vendor };

    public static Group FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Group: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static Group From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Group: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}