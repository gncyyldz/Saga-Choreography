using MassTransit;
using Order.API.Models.Context;
using Shared.Events;
using Model = Order.API.Models;

namespace Order.API.Cunsomers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        readonly OrderDbContext _orderDbContext;

        public PaymentFailedEventConsumer(OrderDbContext orderDbContext)
        {
            _orderDbContext = orderDbContext;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            Model.Order order = await _orderDbContext.FindAsync<Model.Order>(context.Message.OrderId);
            if (order != null)
            {
                order.OrderStatu = Enums.OrderStatus.Fail;
                await _orderDbContext.SaveChangesAsync();
                Console.WriteLine(context.Message.Message);
            }
        }
    }
}
