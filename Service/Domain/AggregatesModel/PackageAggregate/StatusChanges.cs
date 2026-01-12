using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class StatusChange : ValueObject
{
    public bool Status { get; private set; }
    public DateTime? ChangeDate { get; private set; }

    public StatusChange() { }

    public StatusChange(bool status)
    {
        if (status)
        {
            SetStatus();
        }
        else
        {
            ClearStatus();
        }
    }

    private void SetStatus()
    {
        this.Status = true;
        ChangeDate = DateTime.UtcNow;
    }

    private void ClearStatus()
    {
        this.Status = false;
        ChangeDate = null;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        // Using a yield return statement to return each element one at a time
        yield return Status;
        yield return ChangeDate;
    }
}
