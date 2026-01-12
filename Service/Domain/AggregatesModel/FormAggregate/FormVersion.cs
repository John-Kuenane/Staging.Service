namespace Staging.Domain.AggregatesModel.FormAggregate;

public class FormVersion
    : Entity
{
    public Version Version { get; private set; }
    public string  Comment { get; private set; }

    protected FormVersion()
    {
    }

    public FormVersion(string comment, Version newTemplateVersion) : this()
    {
        Comment = comment;

        Version = newTemplateVersion;
    }
}