using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.FormAggregate;

public class ElementType
        : Enumeration
{
    public static ElementType CustomAttribute = new ElementType(1, "CustomAttribute");
    public static ElementType Label = new ElementType(2, "Label");

    public ElementType(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<ElementType> List() =>
        new[] { CustomAttribute, Label };

    public static ElementType FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Element Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static ElementType From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Element Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}