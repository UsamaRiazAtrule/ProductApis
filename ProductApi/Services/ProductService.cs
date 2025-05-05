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

        //public async Task<List<ProductDto>> AllProducts(int pageSize, int pageNumber)
        //{
        //    try
        //    {
        //        var products = await _context.product.ToListAsync();

        //        if (products == null || products.Count == 0)
        //            return null;

        //        var productDtos = products.Select(p => new ProductDto
        //        {
        //            Id = p.Id,
        //            ProductData = p.ProductData
        //        }).ToList();

        //        return productDtos;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //public async Task<List<ProductDto>> AllProductss(int pageNumber, int pageSize, string search)
        //{
        //    try
        //    {

        //        var skip = (pageNumber - 1) * pageSize;
        //        int totalCount = 0;

        //        if (!string.IsNullOrEmpty(search))
        //        {
        //            var cleanedSearch = Regex.Replace(search, @"[^\w\s]", " ");
        //            var terms = cleanedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        //                                     .Select(word => word.Trim())
        //                                     .Where(word => !string.IsNullOrWhiteSpace(word))
        //                                     .Select(word => word + ":*")
        //                                     .ToList();

        //            if (!terms.Any())
        //                return new List<ProductDto>();

        //            var tsQuery = string.Join(" & ", terms);

        //            var query = $@"
        //        SELECT * FROM product 
        //        WHERE to_tsvector('english',
        //            COALESCE(product_data->>'title', '') || ' ' ||
        //            COALESCE(product_data->>'type', '') || ' ' ||
        //            COALESCE(product_data->>'description', '') || ' ' ||
        //            COALESCE(product_data->>'handle', '') || ' ' ||
        //            COALESCE((SELECT string_agg(elem, ' ') FROM jsonb_array_elements_text(product_data->'tags') AS elem), '')) 
        //        @@ to_tsquery('english', {{0}}) 
        //        ORDER BY id 
        //        LIMIT {{1}} OFFSET {{2}}";


        //            var products = await _context.product
        //                                         .FromSqlRaw(query, tsQuery, pageSize, skip)
        //                                         .ToListAsync();
        //            totalCount = products.Count();
        //            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        //            var productDtos = products.Select(p => new ProductDto
        //            {
        //                Id = p.Id,
        //                ProductData = p.ProductData
        //            }).ToList();



        //            return productDtos ?? new List<ProductDto>();
        //        }
        //        else
        //        {

        //            var products = await _context.product
        //                                          .Skip(skip)  // Skip the previous pages' data
        //                                          .Take(pageSize)  // Take only the desired page size
        //                                          .ToListAsync();

        //            var productDtos = products.Select(p => new ProductDto
        //            {
        //                Id = p.Id,
        //                ProductData = p.ProductData
        //            }).ToList();

        //            return productDtos ?? new List<ProductDto>();
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        throw new Exception("An error occurred while fetching products.", ex);
        //    }
        //}
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

                    // Get the total count of products based on the search query (count query)
                    var countQuery = $@"
                SELECT *
                FROM product 
                WHERE to_tsvector('english',
                    COALESCE(product_data->>'title', '') || ' ' ||
                    COALESCE(product_data->>'type', '') || ' ' ||
                    COALESCE(product_data->>'description', '') || ' ' ||
                    COALESCE(product_data->>'handle', '') || ' ' ||
                    COALESCE((SELECT string_agg(elem, ' ') FROM jsonb_array_elements_text(product_data->'tags') AS elem), '')) 
                @@ to_tsquery('english', {{0}})";


                    var query = $@"
                SELECT * FROM product 
                WHERE to_tsvector('english',
                    COALESCE(product_data->>'title', '') || ' ' ||
                    COALESCE(product_data->>'type', '') || ' ' ||
                    COALESCE(product_data->>'description', '') || ' ' ||
                    COALESCE(product_data->>'handle', '') || ' ' ||
                    COALESCE((SELECT string_agg(elem, ' ') FROM jsonb_array_elements_text(product_data->'tags') AS elem), '')) 
                @@ to_tsquery('english', {{0}}) 
                ORDER BY id 
                LIMIT {{1}} OFFSET {{2}}";

                    var countResult = await _context.product.FromSqlRaw(countQuery, tsQuery).ToListAsync();
                    totalCount = countResult.Count() > 0 ? countResult.Count() : 0;

                    var products = await _context.product
                                                 .FromSqlRaw(query, tsQuery, pageSize, skip)
                                                 .ToListAsync();

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
                else
                {
                    // If search is empty, just return all products with pagination
                    totalCount = await _context.product.CountAsync();

                    var products = await _context.product
                                                  .Skip(skip)  // Skip the previous pages' data
                                                  .Take(pageSize)  // Take only the desired page size
                                                  .ToListAsync();

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
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching products.", ex);
            }
        }
        public async Task<ProductDto>SearchProductById(int id)
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

    }
}
