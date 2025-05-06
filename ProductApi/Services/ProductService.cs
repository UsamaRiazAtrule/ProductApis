using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
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
        public async Task<PaginatedProductDto> AllProducts(int pageNumber, int pageSize, string search)
        {
            try
            {
                var skip = (pageNumber - 1) * pageSize;

                int totalCount = 0;

                if (!string.IsNullOrEmpty(search))
                {
                    var cleanedSearch = Regex.Replace(search, @"[^\w\s]", " ");
                    var terms = cleanedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                             .Select(word => word.Trim())
                                             .Where(word => !string.IsNullOrWhiteSpace(word))
                                             .Select(word => word + ":*")
                                             .ToList();

                    if (!terms.Any())
                        return new PaginatedProductDto();

                    var tsQuery = string.Join(" & ", terms);

                    var query = $@"
                     SELECT * FROM product 
                     WHERE to_tsvector('english',
                       COALESCE(product_data->>'title', '') || ' ' ||
                       COALESCE(product_data->>'type', '') || ' ' ||
                       COALESCE(product_data->>'description', '') || ' ' ||
                       COALESCE(product_data->>'handle', '') || ' ' ||
                       COALESCE((SELECT string_agg(elem, ' ') FROM jsonb_array_elements_text(product_data->'tags') AS elem), '')) 
                    @@ to_tsquery('english', {{0}}) 
                    ORDER BY id ";

                    var countResult = await _context.product.FromSqlRaw(query, tsQuery).ToListAsync();
                    totalCount = countResult.Count() > 0 ? countResult.Count() : 0;

                    var products = await _context.product.FromSqlRaw(query + "LIMIT {1} OFFSET {2}", tsQuery, pageSize, skip).ToListAsync();

                    return ReturnToPaginatedProductDTO(totalCount, products, pageSize, totalCount);

                }
                else
                {

                    totalCount = await _context.product.CountAsync();
                    var products = await _context.product.Skip(skip).Take(pageSize).ToListAsync();

                    return ReturnToPaginatedProductDTO(totalCount, products, pageSize, totalCount);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching products.", ex);
            }
        }
        public async Task<ProductDto> SearchProductById(int id)
        {
            try
            {
                var product = await _context.product.Where(p => p.Id == id).FirstOrDefaultAsync();

                if (product == null)
                    return null;

                var productDto = new ProductDto()
                {
                    Id = product.Id,
                    ProductData = product.ProductData
                };
                return productDto;

            }
            catch (Exception)
            {

                throw;
            }
        }

        private PaginatedProductDto ReturnToPaginatedProductDTO(int count, List<Product> products, int pageSize, int totalCount)
        {
            try
            {
                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductData = p.ProductData
                }).ToList();

                // Calculate total pages
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return new PaginatedProductDto
                {
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    Products = productDtos
                };
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
