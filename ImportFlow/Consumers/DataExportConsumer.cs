using ImportFlow.Domain;
using ImportFlow.Domain.ModelsV2;
using ImportFlow.Domain.Repositories.V2;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Consumers;

public class DataExportConsumer(IStateRepositoryV2<ImportEvent> repository) : IMessageConsumer<TransformationFinished>
{
    public async Task Consume(ConsumeContext<TransformationFinished> context)
    {
        var random = new Random();
        var number = random.Next(1, 10);
        
        var causationId = context.Message.EventId;
        
        var state = StateV2.Create(
            StepsName.DateExport,
            context.Message.CorrelationId,
            causationId, 1);

        await repository.AddAsync(state);
        
        await Task.Delay(10000);

        if (number == 1)
        {
            throw new Exception($"Something went wrong in Data Export {DateTime.Now.TimeOfDay}");
        }
        
        
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