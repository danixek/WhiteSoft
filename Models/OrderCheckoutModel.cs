using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace WhiteSoft.Models
{
    public class OrderCheckoutModel
    {
        [Required]
        public string CustomerName { get; set; } = "";
        [Required]
        public string CustomerEmail { get; set; } = "";
        public List<CookieCart>? Cart { get; set; }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
