namespace Order.API.Models.ViewModels
{
    public class OrderItemVM
    {
        public int Count { get; set; }
        public decimal Price { get; set; }
        public string ProductId { get; set; }
    }
}
