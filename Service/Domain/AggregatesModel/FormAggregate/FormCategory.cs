namespace Staging.Domain.AggregatesModel.FormAggregate;

public class FormCategory
    : Entity
{
    public string ShortName { get; private set; }
    public string FriendlyName { get; private set; }
    public string Help { get; private set; }
    public int Order { get; private set; }

    public Version VersionCreated { get; private set; }
    public Version VersionModified { get; private set; }
    public Version VersionDeleted { get; private set; }

    public string ExtendableTypeName { get; private set; }


    private List<FormElement> _elements;
    public IEnumerable<FormElement> Elements => _elements.AsReadOnly();

    protected FormCategory()
    {
        _elements = new List<FormElement>();
    }

    public FormCategory(string shortName, string friendlyName, string help, int order, string extendableTypeName, Version currentTemplateVersion) : this()
    {
        ShortName = !string.IsNullOrWhiteSpace(shortName) ? shortName : throw new ArgumentNullException(nameof(shortName));
        FriendlyName = !string.IsNullOrWhiteSpace(friendlyName) ? friendlyName : throw new ArgumentNullException(nameof(friendlyName));
        Help = help;
        Order = order;
        ExtendableTypeName = extendableTypeName;

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

    public void ChangeOrder(int newOrder)
    {
        if (newOrder < 1)
        {
            throw new DomainException($"Invalid order specified for Element {Id}");
        }
        Order = newOrder;
    }
}