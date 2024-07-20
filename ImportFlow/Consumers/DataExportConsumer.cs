using ImportFlow.Domain;
using ImportFlow.Domain.Repositories;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Consumers;

public class DataExportConsumer(IStateRepository<DataExported> repository) : IMessageConsumer<TransformationFinished>
{
    public async Task Consume(ConsumeContext<TransformationFinished> context)
    {
        var random = new Random();
        var number = random.Next(1, 10);
        
        var causationId = context.Message.EventId;
        
        var state = Domain.State<DataExported>.Create(
            StepsName.DateExport,
            context.Message.CorrelationId,
            causationId, 1);

        await repository.AddAsync(state);

        // if (number == 1)
        // {
        //     throw new Exception("Something went wrong");
        // }
        
        var @event = new DataExported
        {
            CausationId = causationId,
            CorrelationId =context.Message.CorrelationId,
            Number = 1
        };
        
        await repository.PublishedAsync(@event);
        await repository.SucceedAsync(@event);

        // for (var i = 0; i < 4; i++)
        // {
        //     var @event = new DataExported
        //     {
        //         CausationId = context.Message.EventId,
        //         CorrelationId =context.Message.CorrelationId,
        //         Number = i + 1
        //     };
        //     // ...
        //
        //     await _repository.PublishedAsync(@event, StepsName.SupplierFiles);
        //     // await context.Publish(@event);
        // }
    }
}