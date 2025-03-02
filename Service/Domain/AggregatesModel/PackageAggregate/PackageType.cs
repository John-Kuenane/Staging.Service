using Staging.Domain.SeedWork;

namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageType
        : Enumeration
{
    public static PackageType DataListing = new PackageType(1, "Data Listing");
    public static PackageType DataCollection = new PackageType(2, "Data Collection");
    public static PackageType Enrollment = new PackageType(3, "Enrollment");
    public static PackageType DigitalPaymentEnrollment = new PackageType(4, "Digital Payment Enrollment");
    public static PackageType CommunityValidation = new PackageType(5, "Community Validation");
    public static PackageType Unassigned = new PackageType(6, "Unassigned");
    
public PackageType(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<PackageType> List() =>
        new[] { DataListing, DataCollection, Enrollment, DigitalPaymentEnrollment, CommunityValidation, Unassigned };

    public static PackageType FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Package Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static PackageType From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Package Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}