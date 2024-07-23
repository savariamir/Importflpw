namespace ImportFlow.Framework.Domain;

public class Message
{
    public Message(Guid eventId, string errorMessage)
    {
        EventId = eventId;
        ErrorMessage = errorMessage;
        CreatedAt = DateTime.Now;
    }

    public Message(Guid eventId)
    {
        EventId = eventId;
        CreatedAt = DateTime.Now;
    }

    public Guid EventId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ErrorMessage { get; private set; }
}