using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.FormAggregate;

public class OperatorType
        : Enumeration
{
    public static OperatorType EqualTo = new OperatorType(1, "Is Equal To");
    public static OperatorType NotEqualTo = new OperatorType(2, "Is Not Equal To");
    public static OperatorType ContainsWords = new OperatorType(3, "Contains Words");
    public static OperatorType NotContainsWords = new OperatorType(4, "Does Not Contain Words");
    public static OperatorType In = new OperatorType(5, "In");
    public static OperatorType NotIn = new OperatorType(6, "Not In");
    public static OperatorType IsEmpty = new OperatorType(7, "Is Empty");
    public static OperatorType IsNotEmpty = new OperatorType(8, "Is Not Empty");
    public static OperatorType LessThan = new OperatorType(9, "Is Less Than");
    public static OperatorType LessThanOrEqualTo = new OperatorType(10, "Is Less Than Or Equal To");
    public static OperatorType GreaterThan = new OperatorType(11, "Is Greater Than");
    public static OperatorType GreaterThanOrEqualTo = new OperatorType(12, "Is Greater Than Or Equal To");

    public OperatorType(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<OperatorType> List() =>
        new[] { EqualTo, NotEqualTo, ContainsWords, NotContainsWords, In, NotIn, IsEmpty, IsNotEmpty, LessThan, LessThanOrEqualTo, GreaterThan, GreaterThanOrEqualTo };

    public static OperatorType FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Operator Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static OperatorType From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Operator Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}
