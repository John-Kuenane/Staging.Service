using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class DataFlagSubType
        : Enumeration
{
    public static DataFlagSubType Custom = new DataFlagSubType(1, "Custom");
    public static DataFlagSubType PMTCorrectionRequired = new DataFlagSubType(2, "PMT Correction Required");
    public static DataFlagSubType PMTScreeningRequired = new DataFlagSubType(3, "PMT Screening Required");
    public static DataFlagSubType NICRLookUpFailed = new DataFlagSubType(4, "NICR LookUp Failed");
    
    public DataFlagSubType(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<DataFlagSubType> List() =>
        new[] { Custom, PMTCorrectionRequired, PMTScreeningRequired, NICRLookUpFailed };

    public static DataFlagSubType FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Data Flag Sub Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static DataFlagSubType From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Data Flag Sub Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}