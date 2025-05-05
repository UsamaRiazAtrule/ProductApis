using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProductApi.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<JsonDocument>> AllProducts()
        {
            try
            {
                var products = await _context.product.Select(p => p.ProductData).ToListAsync();
                if (products is not null)
                    return products;
                return new List<JsonDocument>();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<JsonDocument>> SearchAllProducts(string search)
        {
            try
            {
                var cleanedSearch = Regex.Replace(search, @"[^\w\s]", " ");
                var terms = cleanedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(word => word.Trim())
                                         .Where(word => !string.IsNullOrWhiteSpace(word))
                                         .Select(word => word + ":*")
                                         .ToList();
                if (!terms.Any())
                    return new List<JsonDocument>();

                var tsQuery = string.Join(" & ", terms);

                var query = @"SELECT * FROM product WHERE to_tsvector('english',
                            COALESCE(product_data->>'title', '') || ' ' ||
                            COALESCE(product_data->>'type', '') || ' ' ||
                            COALESCE(product_data->>'description', '') || ' ' ||
                            COALESCE(product_data->>'handle', '') || ' ' ||
                            COALESCE((SELECT string_agg(elem, ' ') FROM jsonb_array_elements_text(product_data->'tags') AS elem), '')) @@ to_tsquery('english', {0})";

                var products = await _context.product
                                             .FromSqlRaw(query, tsQuery)
                                             .Select(p => p.ProductData)
                                             .ToListAsync();

                return products ?? new List<JsonDocument>();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
