using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using MassTransit;
using Retry = ImportFlow.Events.Retry;

namespace ImportFlow.Infrastructure;

public class MessageRePublisher(IStateRepository<ImportEvent> repository, IBus bus)
{
    public async Task ResendAsync(MessageCommand command)
    {
        var states = await repository.GetAsync(command.ImportFlowId);

        foreach (var state in states)
        {
            var @event = state.Events.FirstOrDefault(p => p.EventId == command.EventId);
            if (@event is null) continue;

            var newEvent = CloneEvent((dynamic)@event);
            
            await repository.AddEventAsync(newEvent);
            await bus.Publish(newEvent);
            break;
        }
    }

    private static InitialLoadFinished CloneEvent(InitialLoadFinished @event)
    {
        return new InitialLoadFinished
        {
            CorrelationId = @event.CorrelationId,
            CausationId = @event.CausationId,
            Number = @event.Number,
            Retry = new Retry("Manual Retry", "Amir Savari", @event.EventId)
        };
    }
    
    private static TransformationFinished CloneEvent(TransformationFinished @event)
    {
        return new TransformationFinished
        {
            CorrelationId = @event.CorrelationId,
            CausationId = @event.CausationId,
            Number = @event.Number,
            Retry = new Retry("Manual Retry", "Amir Savari", @event.EventId)
        };
    }
    
    private static DataExported CloneEvent(DataExported @event)
    {
        return new DataExported
        {
            CorrelationId = @event.CorrelationId,
            CausationId = @event.CausationId,
            Number = @event.Number,
            Retry = new Retry("Manual Retry", "Amir Savari", @event.EventId)
        };
    }
    
    private static SupplierFilesDownloaded CloneEvent(SupplierFilesDownloaded @event)
    {
        return new SupplierFilesDownloaded
        {
            CorrelationId = @event.CorrelationId,
            CausationId = @event.CausationId,
            Number = @event.Number,
            Retry = new Retry("Manual Retry", "Amir Savari", @event.EventId)
        };
    }
}

public class MessageCommand
{
    public Guid ImportFlowId { get; set; }
    public Guid EventId { get; set; }
}