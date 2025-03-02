using Staging.Domain.SeedWork;

namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class DataFlagType
        : Enumeration
{
    public static DataFlagType Custom = new DataFlagType(1, "Custom");
    public static DataFlagType PMTCorrectionRequired = new DataFlagType(2, "PMT Correction Required");
    public static DataFlagType PMTScreeningRequired = new DataFlagType(3, "PMT Screening Required");
    public static DataFlagType NICRLookUpFailed = new DataFlagType(4, "NICR LookUp Failed");
    
public DataFlagType(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<DataFlagType> List() =>
        new[] { Custom, PMTCorrectionRequired, PMTScreeningRequired, NICRLookUpFailed };

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