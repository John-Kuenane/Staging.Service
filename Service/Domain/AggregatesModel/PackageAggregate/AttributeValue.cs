using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class AttributeValue : ValueObject
{
    public string SelectionKey { get; private set; }
    public string SelectionValue { get; private set; }

    public AttributeValue() { }

    public AttributeValue(string selectionKey, string selectionValue)
    {
        SelectionKey = selectionKey;
        SelectionValue = selectionValue;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        // Using a yield return statement to return each element one at a time
        yield return SelectionKey;
        yield return SelectionValue;
    }
}
