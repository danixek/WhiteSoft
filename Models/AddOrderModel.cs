namespace WhiteSoft.Models
{
    public class AddOrderModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public List<AddOrderItemModel> Items { get; set; } = new();
    }

    public class AddOrderItemModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }


}
