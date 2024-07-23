namespace ImportFlow.Events;

public class Retry(string reason, string retryBy, Guid causationId)
{
    public Guid CausationId { private set; get; } = causationId;
    public string Reason { private set; get; } = reason;
    public string RetryBy {private set; get; } = retryBy;
}