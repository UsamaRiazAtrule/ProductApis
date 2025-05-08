using ProductApi.DTOs;
using ProductApi.Models;
using System.Text.Json;

namespace ProductApi.Services
{
    public interface IProductService
    {
        Task<PaginatedProductDto> AllProducts(int pageNumber, int pageSize, string search, string sortByPriceDirection);
        Task<ProductDto> SearchProductById(int id);
        Task<PaginatedProductDto> GetProductsByBrand(int brand_id, int pageNumber, int pageSize, string sortByPrice);
        Task<List<Brand>> Brands();
    }
}
