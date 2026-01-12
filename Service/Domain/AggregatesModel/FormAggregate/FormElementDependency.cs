namespace Staging.Domain.AggregatesModel.FormAggregate;

public class FormElementDependency
    : Entity
{
    public Guid ComparisonFormElementId { get; private set; }
    public int DependencyRelationshipTypeId { get; private set; }
    public int OperatorTypeId { get; private set; }
    public string ComparisonValue { get; private set; }

    protected FormElementDependency()
    {
    }

    public FormElementDependency(Guid comparisonFormElementId, int dependencyRelationshipTypeId, int operatorTypeId, string comparisonValue) : this()
    {
        ComparisonFormElementId = comparisonFormElementId;
        DependencyRelationshipTypeId = dependencyRelationshipTypeId;
        OperatorTypeId = operatorTypeId;
        ComparisonValue = comparisonValue;
    }

    public void ChangeDetails(int dependencyRelationshipTypeId, int operatorTypeId, string comparisonValue)
    {
        DependencyRelationshipTypeId = dependencyRelationshipTypeId;
        OperatorTypeId = operatorTypeId;
        ComparisonValue = !string.IsNullOrWhiteSpace(comparisonValue) ? comparisonValue : throw new ArgumentNullException(nameof(comparisonValue));
    }
}