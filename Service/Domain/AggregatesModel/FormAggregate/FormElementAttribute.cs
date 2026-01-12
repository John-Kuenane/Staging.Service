namespace Staging.Domain.AggregatesModel.FormAggregate;

public class FormElementAttribute
    : Entity
{
    public int AttributeTypeId { get; private set; }
    public string Value { get; private set; }

    protected FormElementAttribute()
    {
    }

    public FormElementAttribute(AttributeType attributeType, string attributeValue) : this()
    {
        AttributeTypeId = attributeType.Id;
        Value = attributeValue;
    }

    public void UpdateValue(string attributeValue)
    {
        Value = attributeValue;
    }
}