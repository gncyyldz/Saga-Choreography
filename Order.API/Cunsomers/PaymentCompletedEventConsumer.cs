using MassTransit;
using Order.API.Models.Context;
using Shared.Events;
using Model = Order.API.Models;

namespace Order.API.Cunsomers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        readonly OrderDbContext _orderDbContext;
        public PaymentCompletedEventConsumer(OrderDbContext orderDbContext)
        {
            _orderDbContext = orderDbContext;
        }
        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            Model.Order order = await _orderDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.OrderStatu = Enums.OrderStatus.Completed;
                await _orderDbContext.SaveChangesAsync();
            }
        }
    }
}
