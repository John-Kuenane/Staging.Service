using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.FormAggregate;

public class AttributeType
        : Enumeration
{
    public static AttributeType Mandatory = new AttributeType(1, "Mandatory");
    public static AttributeType MaxLength = new AttributeType(2, "MaxLength");
    public static AttributeType RegEx = new AttributeType(3, "RegEx");
    public static AttributeType Decimals = new AttributeType(4, "Decimals");
    public static AttributeType MinSize = new AttributeType(5, "MinSize");
    public static AttributeType MaxSize = new AttributeType(6, "MaxSize");
    public static AttributeType ValueList = new AttributeType(7, "ValueList");
    public static AttributeType DisplayAgeInYears = new AttributeType(8, "DisplayAgeInYears");

    public AttributeType(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<AttributeType> List() =>
        new[] { Mandatory, MaxLength, RegEx, Decimals, MinSize, MaxSize, ValueList, DisplayAgeInYears };

    public static AttributeType FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Attribute Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static AttributeType From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Attribute Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}