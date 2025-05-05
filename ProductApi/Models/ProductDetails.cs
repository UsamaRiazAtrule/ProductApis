using System.Text.Json.Nodes;

namespace ProductApi.Models
{
    public class ProductDetails
    {
        public int Id { get; set; }
        public string ShopifyProductId { get; set; }
        public string Url { get; set; }
        public JsonArray Tags { get; set; }  // or JsonNode depending on structure
        public bool PriceVaries { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public string Title { get; set; }
        public string Handle { get; set; }
        public JsonArray ImagesUrls { get; set; }  // or JsonNode depending on structure
        public string Vendor { get; set; }
        public bool Available { get; set; }
        public decimal PriceMax { get; set; }
        public decimal PriceMin { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Description { get; set; }
        public string FeaturedImage { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public int DomainId { get; set; }
        public int ProductId { get; set; }

        // Optional: Navigation properties if using Entity Framework
        // public Merchant Domain { get; set; }
        // public Product Product { get; set; }
    }
}
