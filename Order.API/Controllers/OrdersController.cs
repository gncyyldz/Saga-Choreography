using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Enums;
using Order.API.Models;
using Order.API.Models.Context;
using Order.API.Models.ViewModels;
using Shared.Events;
using Shared.Messages;
using Model = Order.API.Models;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        readonly OrderDbContext _orderDbContext;
        readonly IPublishEndpoint _publishEndpoint;
        public OrdersController(OrderDbContext orderDbContext, IPublishEndpoint publishEndpoint)
        {
            _orderDbContext = orderDbContext;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderVM model)
        {
            Model::Order order = new Model::Order()
            {
                BuyerId = Guid.TryParse(model.BuyerId, out Guid _buyerId) ? _buyerId : Guid.NewGuid(),
                OrderItems = model.OrderItems.Select(oi => new OrderItem()
                {
                    Count = oi.Count,
                    Price = oi.Price,
                    ProductId = Guid.TryParse(oi.ProductId, out Guid _productId) ? _productId : Guid.NewGuid()
                }).ToList(),
                OrderStatu = OrderStatus.Suspend,
                TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
                CreatedDate = DateTime.UtcNow
            };

            await _orderDbContext.AddAsync(order);
            await _orderDbContext.SaveChangesAsync();

            OrderCreatedEvent orderCreatedEvent = new()
            {
                OrderId = order.Id,
                BuyerId = order.BuyerId,
                TotalPrice = order.TotalPrice,
                OrderItems = order.OrderItems.Select(oi => new OrderItemMessage()
                {
                    Price = oi.Price,
                    Count = oi.Count,
                    ProductId = oi.ProductId
                }).ToList()
            };
            await _publishEndpoint.Publish(orderCreatedEvent);
            return Ok(true);
        }
    }
}
