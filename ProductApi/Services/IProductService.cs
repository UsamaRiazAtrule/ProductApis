using ProductApi.DTOs;
using ProductApi.Models;
using System.Text.Json;

namespace ProductApi.Services
{
    public interface IProductService
    {
        Task<PaginatedDto> AllProducts(int pageNumber, int pageSize, string search, string sortByPriceDirection);
        Task<ProductDto> SearchProductById(int id);
        Task<PaginatedDto> GetProductsByBrand(int brand_id, int pageNumber, int pageSize);
        Task<List<Brand>> Brands();
    }
}
