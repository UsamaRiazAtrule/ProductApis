using ProductApi.Models;

namespace ProductApi.DTOs
{
    public class PaginatedProductDto
    {
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public string? BrandName { get; set; }
        public List<ProductDto> Products { get; set; }
    }
}
