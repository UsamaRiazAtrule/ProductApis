using ProductApi.Models;

namespace ProductApi.DTOs
{
    public class PaginatedDto
    {
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<ProductDto> Products { get; set; }
    }
}
