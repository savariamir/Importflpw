namespace ImportFlow.Domain;

public class FailedEvent(Guid eventId, string errorMessage)
{
    public Guid EventId { get; set; } = eventId;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string ErrorMessage { get; private set; } = errorMessage;
}