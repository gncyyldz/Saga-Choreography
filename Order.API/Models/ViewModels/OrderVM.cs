namespace Order.API.Models.ViewModels
{
    public class OrderVM
    {
        public string BuyerId { get; set; }
        public List<OrderItemVM> OrderItems { get; set; }
    }
}
