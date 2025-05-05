using ProductApi.DTOs;
using ProductApi.Models;
using System.Text.Json;

namespace ProductApi.Services
{
    public interface IProductService
    {
        Task<PaginatedProductDto> AllProducts(int pageNumber, int pageSize, string search);
        Task<ProductDto> SearchProductById(int id);
    }
}
