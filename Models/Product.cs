namespace WhiteSoft.Models
{
    public class Product
    {
        public int Id { get; set; }               // primary key
        public string Name { get; set; } = "";    // nme of product
        public string Type { get; set; } = "";    // category
        public decimal Price { get; set; }        // cost of product
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPinned { get; set; } // for main page

        // For services, e.g. hosting, repair
        // null means the product is not a service (capacity is not limited)
        public int? MaxCapacity { get; internal set; }
    }
}
