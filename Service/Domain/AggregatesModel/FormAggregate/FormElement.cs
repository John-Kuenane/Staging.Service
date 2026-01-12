namespace Staging.Domain.AggregatesModel.FormAggregate;

public class FormElement
    : Entity
{
    public int ElementTypeId { get; private set; }
    public string ShortName { get; private set; }
    public string FriendlyName { get; private set; }
    public string Help { get; private set; }
    public int Order { get; private set; }

    public Guid? CustomAttributeConfigurationGuid { get; private set; }
    public int CustomAttributeTypeId { get; private set; }
    public string Category { get; private set; }
    public string AttributeKey { get; private set; }
    public string AttributeCode { get; private set; }
    public bool IsRequired { get; private set; }
    public int? StringMaxLength { get; private set; }
    public int? NumericMinValue { get; private set; }
    public int? NumericMaxValue { get; private set; }
    public bool FutureDateOnly { get; private set; }
    public bool PastDateOnly { get; private set; }
    public bool MultipleSelection { get; private set; }
    public string RegEx { get; private set; }
    public bool IsFormula { get; set; }

    public Version VersionCreated { get; private set; }
    public Version VersionModified { get; private set; }
    public Version VersionDeleted { get; private set; }

    private List<FormElementDependency> _dependencies;
    public IEnumerable<FormElementDependency> Dependencies => _dependencies.AsReadOnly();

    private List<FormElementAttribute> _attributes;
    public IEnumerable<FormElementAttribute> Attributes => _attributes.AsReadOnly();

    protected FormElement()
    {
        _dependencies = new List<FormElementDependency>();
        _attributes = new List<FormElementAttribute>();
    }

    public FormElement(ElementType elementType, string shortName, string friendlyName, string help, int order, Version currentTemplateVersion) : this()
    {
        ElementTypeId = elementType.Id;
        ShortName = !string.IsNullOrWhiteSpace(shortName) ? shortName : throw new ArgumentNullException(nameof(shortName));
        FriendlyName = !string.IsNullOrWhiteSpace(friendlyName) ? friendlyName : throw new ArgumentNullException(nameof(friendlyName));
        Help = help;
        Order = order;

        VersionCreated = currentTemplateVersion;
    }

    public void ChangeName(string shortName, string friendlyName, Version currentTemplateVersion)
    {
        ShortName = !string.IsNullOrWhiteSpace(shortName) ? shortName : throw new ArgumentNullException(nameof(shortName));
        FriendlyName = !string.IsNullOrWhiteSpace(friendlyName) ? friendlyName : throw new ArgumentNullException(nameof(friendlyName));

        VersionModified = currentTemplateVersion;
    }

    public void ChangeHelp(string help, Version currentTemplateVersion)
    {
        Help = help;

        VersionModified = currentTemplateVersion;
    }

    public void ChangeCustomAttributes(Guid customAttributeConfigurationGuid, CustomAttributeType customAttributeType, string category, string attributeKey, string attributeCode, bool isRequired, int? stringMaxLength, int? numericMinValue, int? numericMaxValue, bool futureDateOnly, bool pastDateOnly, bool multipleSelection, string regEx, bool isFormula, Version currentTemplateVersion)
    {
        CustomAttributeTypeId = customAttributeType.Id;
        CustomAttributeConfigurationGuid = customAttributeConfigurationGuid;

        Category = category;
        AttributeKey = attributeKey;
        AttributeCode = attributeCode;

        IsRequired = isRequired;
        StringMaxLength = stringMaxLength;
        NumericMinValue = numericMinValue;
        NumericMaxValue = numericMaxValue;
        FutureDateOnly = futureDateOnly;
        PastDateOnly = pastDateOnly;
        MultipleSelection = multipleSelection;
        RegEx = regEx;

        IsFormula = isFormula;

        VersionModified = currentTemplateVersion;
    }

    public void ChangeOrder(int newOrder)
    {
        if (newOrder < 1)
        {
            throw new DomainException($"Invalid order specified for Element {Id}");
        }
        Order = newOrder;
    }

    public FormElementDependency AddDependency(Guid comparisonFormElementId, DependencyRelationshipType dependencyRelationshipType, OperatorType operatorType, string comparisonValue, Version currentTemplateVersion)
    {
        var newFormElementDependency = new FormElementDependency(comparisonFormElementId, dependencyRelationshipType.Id, operatorType.Id, comparisonValue);
        _dependencies.Add(newFormElementDependency);

        VersionModified = currentTemplateVersion;

        return newFormElementDependency;
    }

    public void ChangeDependencyDetails(int dependencyId, DependencyRelationshipType dependencyRelationshipType, OperatorType operatorType, string comparisonValue, Version currentTemplateVersion)
    {
        var dependency = _dependencies.SingleOrDefault(d => d.Id == dependencyId);
        if (dependency == null)
        {
            throw new KeyNotFoundException($"Unable to locate dependency {dependencyId} for element {Id}");
        }

        dependency.ChangeDetails(dependencyRelationshipType.Id, operatorType.Id, comparisonValue);

        VersionModified = currentTemplateVersion;
    }

    public void DeleteDependency(int dependencyId, Version currentTemplateVersion)
    {
        var dependency = _dependencies.SingleOrDefault(d => d.Id == dependencyId);
        if (dependency == null)
        {
            throw new KeyNotFoundException($"Unable to locate dependency {dependencyId} for element {Id}");
        }
        if (dependency.DependencyRelationshipTypeId == DependencyRelationshipType.And.Id 
            || dependency.DependencyRelationshipTypeId == DependencyRelationshipType.Or.Id)
        {
            if(_dependencies.Count > 1)
            {
                throw new DomainException($"Unable to delete dependency {dependencyId} with relationship defined for element {Id}");
            }
        }

        _dependencies.Remove(dependency);

        VersionModified = currentTemplateVersion;
    }
}