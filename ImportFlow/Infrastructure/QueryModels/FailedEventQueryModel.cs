namespace ImportFlow.Infrastructure.QueryModels;

public class FailedEventQueryModel
{
    public Guid EventId { get; set; }

    public DateTime CreatedAt { get; set; }

    public required string ErrorMessage { get;  set; }
}