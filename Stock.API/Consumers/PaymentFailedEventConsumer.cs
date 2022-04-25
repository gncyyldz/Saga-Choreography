using MassTransit;
using Shared.Events;
using Shared.Messages;
using Stock.API.Services;
using Model = Stock.API.Models;
using MongoDB.Driver;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        readonly MongoDbService _mongoDbService;
        public PaymentFailedEventConsumer(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var collection = _mongoDbService.GetCollection<Model.Stock>();
            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
            {
                Model.Stock stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                if (stock != null)
                {
                    stock.Count += orderItem.Count;
                    await collection.FindOneAndReplaceAsync(s => s.ProductId == orderItem.ProductId, stock);
                }
            }
        }
    }
}
