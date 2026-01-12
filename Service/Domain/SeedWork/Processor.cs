namespace Staging.Domain.SeedWork;

public abstract class Processor
{
    protected Processor()
    {
        Status = ProcessorStatus.Queued;
        StatusDate = DateTime.UtcNow;
    }

    public abstract string ActivityType { get; }

    public string Name { get; set; }
    public ProcessorStatus Status { get; private set; }
    public string StatusMessage { get; private set; }
    public DateTime StatusDate { get; private set; }

    public void MarkAsSuccessfullyCompleted()
    {
        Status = ProcessorStatus.Complete;
        StatusMessage = "Activity Successfully Completed.";
        StatusDate = DateTime.UtcNow;
    }

    public void MarkAsBlocked(string message)
    {
        Status = ProcessorStatus.Blocked;
        StatusMessage = message;
        StatusDate = DateTime.UtcNow;
    }

    public void MarkAsCompleteWithErrors(string message)
    {
        Status = ProcessorStatus.CompleteWithErrors;
        StatusMessage = message;
        StatusDate = DateTime.UtcNow;
    }

    public void MarkAsInProgress()
    {
        if (Status == ProcessorStatus.InProgress) return;

        Status = ProcessorStatus.InProgress;
        StatusDate = DateTime.UtcNow;
    }
}

