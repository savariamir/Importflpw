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

        if (number == 1)
        {
            throw new Exception("Something went wrong");
        }

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