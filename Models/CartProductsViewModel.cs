using System.Text.Json;

namespace WhiteSoft.Models
{
    public class CartProductsViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public ProductDetails Details { get; set; } = new ProductDetails();
    }
    public class ProductDetails
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }

}
