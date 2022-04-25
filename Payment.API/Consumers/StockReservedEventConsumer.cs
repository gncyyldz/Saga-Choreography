using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        readonly IPublishEndpoint _publishEndpoint;
        public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            if (true)
            {
                PaymentCompletedEvent paymentCompletedEvent = new()
                {
                    OrderId = context.Message.OrderId
                };
                await _publishEndpoint.Publish(paymentCompletedEvent);
                Console.WriteLine("Ödeme başarılı...");
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new()
                {
                    Message = "Bakiye yetersiz...",
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems
                };
                await _publishEndpoint.Publish(paymentFailedEvent);
                Console.WriteLine("Ödeme başarısız...");
            }
        }
    }
}
