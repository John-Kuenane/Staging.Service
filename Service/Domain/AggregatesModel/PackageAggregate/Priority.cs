using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class Priority
        : Enumeration
{
    public static Priority Low = new Priority(1, "Low");
    public static Priority Medium = new Priority(2, "Medium");
    public static Priority High = new Priority(3, "High");
    public static Priority Urgent = new Priority(4, "Urgent");
    
public Priority(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<Priority> List() =>
        new[] { Low, Medium, High, Urgent };

    public static Priority FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Priority: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static Priority From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Priority: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}