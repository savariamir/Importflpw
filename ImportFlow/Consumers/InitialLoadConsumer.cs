using ImportFlow.Domain;
using ImportFlow.Domain.ModelsV2;
using ImportFlow.Domain.Repositories.V2;
using ImportFlow.Events;
using MassTransit;

namespace ImportFlow.Consumers;

public class InitialLoadConsumer(IStateRepositoryV2<InitialLoadFinished> repository)
    : IMessageConsumer<SupplierFilesDownloaded>
{
    public async Task Consume(ConsumeContext<SupplierFilesDownloaded> context)
    {
        var random = new Random();
        var number = random.Next(1, 10);

        var causationId = context.Message.EventId;

        var state =StateV2.Create(
            StepsName.InitialLoad,
            context.Message.CorrelationId,
            causationId, 5);

        await repository.AddAsync(state);

        if (number == 1)
        {
            throw new Exception($"Something went wrong in Initial Load {DateTime.Now.TimeOfDay}");
        }

        for (var i = 0; i < 5; i++)
        {
            var @event = new InitialLoadFinished
            {
                CorrelationId = context.Message.CorrelationId,
                CausationId = causationId,
                Number = i + 1
            };
            // ...

            await repository.PublishedAsync(@event);
            await context.Publish(@event);
        }
    }
}