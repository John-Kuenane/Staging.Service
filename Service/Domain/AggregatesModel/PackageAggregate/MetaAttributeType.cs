using Staging.Domain.SeedWork;

namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class MetaAttributeType
        : Enumeration
{
    public static MetaAttributeType ActivityStarted = new MetaAttributeType(1, "Activity Started");
    public static MetaAttributeType ActivityFinished = new MetaAttributeType(2, "Activity Finished");
    public static MetaAttributeType ActivityDuration = new MetaAttributeType(3, "Activity Duration");
    public static MetaAttributeType GPSLocation = new MetaAttributeType(4, "GPS Location");
    
public MetaAttributeType(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<MetaAttributeType> List() =>
        new[] { ActivityStarted, ActivityFinished, ActivityDuration, GPSLocation };

    public static MetaAttributeType FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Meta Attribute Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static MetaAttributeType From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Meta Attribute Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}