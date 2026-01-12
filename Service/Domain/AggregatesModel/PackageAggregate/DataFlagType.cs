using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class DataFlagType
        : Enumeration
{
    public static DataFlagType DataQualityIssue = new DataFlagType(1, "Data Quality Issue");
    
public DataFlagType(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<DataFlagType> List() =>
        new[] { DataQualityIssue };

    public static DataFlagType FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Data Flag Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static DataFlagType From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Data Flag Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}