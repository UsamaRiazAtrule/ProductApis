using ProductApi.Models;
using System.Text.Json;

namespace ProductApi.Services
{
    public interface IProductService
    {
        Task<List<JsonDocument>> AllProducts();
        Task<List<JsonDocument>> SearchAllProducts(string search);
    }
}
