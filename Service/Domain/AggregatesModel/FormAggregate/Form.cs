namespace Staging.Domain.AggregatesModel.FormAggregate;

public class Form
    : Entity, IAggregateRoot
{
    public string ShortName { get; private set; }
    public string FriendlyName { get; private set; }
    public string UniqueCode { get; private set; }
    public string Help { get; private set; }
    public int FormStatusId { get; private set; }

    public Version CurrentVersion { get; private set; }

    private List<FormCategory> _categories;
    public IEnumerable<FormCategory> Categories => _categories.AsReadOnly();

    private List<FormVersion> _versions;
    public IEnumerable<FormVersion> Versions => _versions.AsReadOnly();

    protected Form()
    {
        _categories = new List<FormCategory>();
        _versions = new List<FormVersion>();
    }

    public Form(string shortName, string friendlyName, string help, string uniqueCode) : this()
    {
        ShortName = !string.IsNullOrWhiteSpace(shortName) ? shortName : throw new ArgumentNullException(nameof(shortName));
        FriendlyName = !string.IsNullOrWhiteSpace(friendlyName) ? friendlyName : throw new ArgumentNullException(nameof(friendlyName));
        Help = help;
        UniqueCode = uniqueCode;

        FormStatusId = FormStatus.Unpublished.Id;

        CurrentVersion = new Version(1, 0);
        _versions.Add(new FormVersion(string.Empty, CurrentVersion));
    }

    public void ChangeName(string shortName, string friendlyName, string uniqueCode)
    {
        ShortName = !string.IsNullOrWhiteSpace(shortName) ? shortName : throw new ArgumentNullException(nameof(shortName));
        FriendlyName = !string.IsNullOrWhiteSpace(friendlyName) ? friendlyName : throw new ArgumentNullException(nameof(friendlyName));
        UniqueCode = !string.IsNullOrWhiteSpace(uniqueCode) ? uniqueCode : throw new ArgumentNullException(nameof(uniqueCode));
    }
}
