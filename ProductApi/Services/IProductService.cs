using ProductApi.DTOs;
using ProductApi.Models;
using System.Text.Json;

namespace ProductApi.Services
{
    public interface IProductService
    {
        Task<PaginatedProductDto> AllProducts(int pageNumber, int pageSize, string search, string sortByPriceDirection);
        Task<ProductDto> SearchProductById(int id);
        Task<List<ProductDto>> GetProductsByBrand(int brand_id);
        Task<List<Brand>> Brands();
    }
}
