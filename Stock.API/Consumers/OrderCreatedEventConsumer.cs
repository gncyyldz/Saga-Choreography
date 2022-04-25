using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Shared.Messages;
using Stock.API.Services;
using Model = Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IPublishEndpoint _publishEndpoint;
        readonly MongoDbService _mongoDbService;

        public OrderCreatedEventConsumer(ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint, MongoDbService mongoDbService)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
            _mongoDbService = mongoDbService;
        }
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();
            IMongoCollection<Model.Stock> collection = _mongoDbService.GetCollection<Model.Stock>();

            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
            {
                stockResult.Add((await collection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count > orderItem.Count)).Any());

                var s = await collection.FindAsync(s => s.ProductId == orderItem.ProductId);
                var c = await (await collection.FindAsync(s => true)).FirstOrDefaultAsync();
            }

            if (stockResult.TrueForAll(sr => sr.Equals(true)))
            {
                foreach (OrderItemMessage orderItem in context.Message.OrderItems)
                {
                    Model.Stock stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                    stock.Count -= orderItem.Count;
                    await collection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
                }

                ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));
                StockReservedEvent stockReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems,
                    TotalPrice = context.Message.TotalPrice,
                };
                await sendEndpoint.Send(stockReservedEvent);
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    Message = "Yetersiz stok"
                };
                await _publishEndpoint.Publish(stockNotReservedEvent);
            }
        }
    }
}
