using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.FormAggregate;

public class CustomAttributeType
        : Enumeration
{
    public static CustomAttributeType None = new CustomAttributeType(1, "None");
    public static CustomAttributeType Numeric = new CustomAttributeType(2, "Numeric");
    public static CustomAttributeType AlphaNumeric = new CustomAttributeType(3, "Alpha Numeric");
    public static CustomAttributeType Selection = new CustomAttributeType(4, "Selection");
    public static CustomAttributeType DateTime = new CustomAttributeType(5, "Date Time");
    public static CustomAttributeType FirstClassProperty = new CustomAttributeType(6, "First Class Property");
    public static CustomAttributeType Calculation = new CustomAttributeType(7, "Calculation");
    public static CustomAttributeType Photo = new CustomAttributeType(8, "Photo");
    public static CustomAttributeType Multiselection = new CustomAttributeType(9, "Multiselection");
    public static CustomAttributeType GPSCoords = new CustomAttributeType(10, "GPSCoords");

    public CustomAttributeType(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<CustomAttributeType> List() =>
        new[] { None, Numeric, AlphaNumeric, Selection, DateTime, FirstClassProperty, Calculation, Photo, Multiselection, GPSCoords };

    public static CustomAttributeType FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Custom Attribute Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static CustomAttributeType From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Custom Attribute Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}