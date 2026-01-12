using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.FormAggregate;

public class FormStatus
        : Enumeration
{
    public static FormStatus Unpublished = new FormStatus(1, "Unpublished");
    public static FormStatus Published = new FormStatus(2, "Published");
    public static FormStatus Deprecated = new FormStatus(3, "Deprecated");

    public FormStatus(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<FormStatus> List() =>
        new[] { Unpublished, Published, Deprecated };

    public static FormStatus FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Form Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static FormStatus From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Form Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}
