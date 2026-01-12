using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PayloadProcessedStatus
        : Enumeration
{
    public static PayloadProcessedStatus NotProcessed = new PayloadProcessedStatus(1, "Not Processed");
    public static PayloadProcessedStatus Processed = new PayloadProcessedStatus(2, "Processed");
    public static PayloadProcessedStatus Failed = new PayloadProcessedStatus(3, "Failed");
    public static PayloadProcessedStatus Pending = new PayloadProcessedStatus(4, "Pending");
    public static PayloadProcessedStatus Processing = new PayloadProcessedStatus(5, "Processing");

    public PayloadProcessedStatus(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<PayloadProcessedStatus> List() =>
        new[] { NotProcessed, Processed, Failed, Pending, Processing };

    public static PayloadProcessedStatus FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Payload Processed Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static PayloadProcessedStatus From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Payload Processed Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}