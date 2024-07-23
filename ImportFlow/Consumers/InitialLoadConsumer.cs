using ImportFlow.Events;
using ImportFlow.Framework;
using ImportFlow.Framework.Domain;
using ImportFlow.Framework.Domain.Repositories;
using MassTransit;
using State = ImportFlow.Framework.Domain.State;

namespace ImportFlow.Consumers;

public class InitialLoadConsumer(IStateRepository<ImportEvent> repository)
    : IMessageConsumer<SupplierFilesDownloaded>
{
    public async Task Consume(ConsumeContext<SupplierFilesDownloaded> context)
    {
        var random = new Random();
        var number = random.Next(1, 10);

        var causationId = context.Message.EventId;

        var state =State.Create(
            StepsName.InitialLoad,
            context.Message.CorrelationId,
            causationId, 5);
        

        await repository.AddAsync(state);

        // if (number == 1)
        // {
        //     throw new Exception($"Something went wrong in Initial Load {DateTime.Now.TimeOfDay}");
        // }

        await Task.Delay(1000);

        for (var i = 0; i < 5; i++)
        {
            var @event = new InitialLoadFinished
            {
                CorrelationId = context.Message.CorrelationId,
                CausationId = causationId,
                Number = i + 1
            };
            // ...

            await repository.PublishingAsync(@event);
            await context.Publish(@event);
        }
    }
}