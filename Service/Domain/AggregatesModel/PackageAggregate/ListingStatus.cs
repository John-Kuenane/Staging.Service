using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class ListingStatus
        : Enumeration
{
    public static ListingStatus NoStatus = new ListingStatus(1, "No Status");
    public static ListingStatus ListingAndCBCSynched = new ListingStatus(2, "Listing And CBC Synched");
    public static ListingStatus NewHouseholdSynched = new ListingStatus(3, "New Household Synched");
    public static ListingStatus DisolvedOrDuplicateHouseholdSynched = new ListingStatus(4, "Disolved Or Duplicate Household Synched");
    
public ListingStatus(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<ListingStatus> List() =>
        new[] { NoStatus, ListingAndCBCSynched, NewHouseholdSynched, DisolvedOrDuplicateHouseholdSynched };

    public static ListingStatus FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Listing Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static ListingStatus From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Listing Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}