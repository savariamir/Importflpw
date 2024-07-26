namespace ImportFlow.Domain;

public class FailedEvent(Guid eventId, string errorMessage)
{
    public Guid EventId { get; } = eventId;

    public DateTime CreatedAt { get; } = DateTime.Now;

    public string ErrorMessage { get; private set; } = errorMessage;
}