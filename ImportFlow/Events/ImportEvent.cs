namespace ImportFlow.Events;

public class ImportEvent
{
    protected ImportEvent()
    {
        CreatedAt = DateTime.Now;
        EventId = Guid.NewGuid();
    }

    public Guid EventId { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTime CreatedAt { get; }
    public int Number { set; get; }
}