using System.ComponentModel.DataAnnotations;

namespace WhiteSoft.Models
{
    public class CartUserViewModel
    {
        [Required]
        public string CustomerName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        public List<CartProductsViewModel> Cart { get; set; } = new();
    }

}
