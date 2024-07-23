namespace ImportFlow.Infrastructure.QueryModels;

public class RetryQueryModel
{
    public Guid? CausationId { set; get; }
    public string? Reason {  set; get; }
    public string? RetryBy { set; get; }
}