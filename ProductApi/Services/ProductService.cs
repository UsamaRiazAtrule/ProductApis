﻿using Microsoft.AspNetCore.Routing.Constraints;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;
using System.Linq;
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

        public async Task<PaginatedProductDto> AllProducts(int pageNumber, int pageSize, string search, string sortByPriceDirection)
        {
            try
            {
                var skip = (pageNumber - 1) * pageSize;
                int totalCount = 0;
                List<Product> products;

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

                    var result = await _context.Database.SqlQuery<int>($@"
                                             SELECT COUNT(id)
                                             FROM products 
                                             WHERE search_vector @@ to_tsquery('english', {tsQuery})
                                             ").ToListAsync();
                    totalCount = result.FirstOrDefault();


                    var baseSearchQuery = @"SELECT * FROM products WHERE search_vector @@ to_tsquery('english', {0})";
                    string orderClause = "";

                    if (!string.IsNullOrEmpty(sortByPriceDirection))
                    {
                        if (sortByPriceDirection.Equals("LTH", StringComparison.OrdinalIgnoreCase))
                            orderClause = "ORDER BY (product_data->>'price')::numeric ASC ";
                        else if (sortByPriceDirection.Equals("HTL", StringComparison.OrdinalIgnoreCase))
                            orderClause = "ORDER BY (product_data->>'price')::numeric DESC ";
                        else
                            orderClause = "ORDER BY id ";
                    }
                    else
                    {
                        orderClause = "ORDER BY id ";
                    }

                    products = await _context.products.FromSqlRaw(baseSearchQuery + orderClause + "LIMIT {1} OFFSET {2}", tsQuery, pageSize, skip).ToListAsync();

                }
                else
                {
                    string baseQuery = "SELECT * FROM products ";
                    string orderClause = "";

                    if (!string.IsNullOrEmpty(sortByPriceDirection))
                    {
                        if (sortByPriceDirection.Equals("LTH", StringComparison.OrdinalIgnoreCase))
                            orderClause = "ORDER BY (product_data->>'price')::numeric ASC ";
                        else if (sortByPriceDirection.Equals("HTL", StringComparison.OrdinalIgnoreCase))
                            orderClause = "ORDER BY (product_data->>'price')::numeric DESC ";
                        else
                            orderClause = "ORDER BY id ";
                    }
                    else
                    {
                        orderClause = "ORDER BY id ";
                    }

                    var fullQuery = baseQuery + orderClause + "LIMIT {0} OFFSET {1}";

                    totalCount = await _context.products.CountAsync();
                    products = await _context.products.FromSqlRaw(fullQuery, pageSize, skip).ToListAsync();

                }
                return ReturnToPaginatedProductDto(products, pageSize, totalCount);
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
                var product = await _context.products.Where(p => p.Id == id).FirstOrDefaultAsync();

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
        public async Task<PaginatedProductDto> GetProductsByBrand(int brand_id, int pageNumber, int pageSize, string sortByPrice)
        {


            try
            {
                var domain = await _context.merchants.Where(m => m.id == brand_id).Select(s => s.domain).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(domain))
                    return new PaginatedProductDto();

                List<Product> products;
                int totalCount;

                if (!string.IsNullOrEmpty(sortByPrice) && (sortByPrice.Equals("LTH", StringComparison.OrdinalIgnoreCase) || sortByPrice.Equals("HTL", StringComparison.OrdinalIgnoreCase)))
                {
                    var sortDirection = sortByPrice.Equals("LTH", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";

                    var rawSql = _context.products
                        .FromSqlRaw(@"
                                     SELECT * FROM products
                                     WHERE ""merchant_domain"" = {0}
                                     ORDER BY (""product_data""->>'price')::numeric " + sortDirection + @"
                                     OFFSET {1}
                                     LIMIT {2}", domain, (pageNumber - 1) * pageSize, pageSize);

                    products = await rawSql.ToListAsync();

                    totalCount = await _context.products.Where(p => p.MerchantDomain == domain).CountAsync();
                }
                else
                {
                    var allProducts = _context.products.Where(p => p.MerchantDomain == domain).OrderBy(p => p.Id);
                    totalCount = await allProducts.CountAsync();
                    products = await allProducts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
                }

                var productDto = products.Select(p => new ProductDto()
                {
                    Id = p.Id,
                    ProductUrl = $"{p.MerchantDomain}{p.ProductUrl}",
                    ProductData = p.ProductData
                }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return new PaginatedProductDto
                {
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    Products = productDto
                };
            }
            catch (Exception)
            {
                throw;
            }

        }

        private PaginatedProductDto ReturnToPaginatedProductDto(List<Product> products, int pageSize, int totalCount)
        {
            try
            {
                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductUrl = $"{p.MerchantDomain}{p.ProductUrl}",
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

        public async Task<List<Brand>> Brands()
        {
            try
            {
                var brands = await _context.merchants.ToListAsync();
                var sortedBrands = brands
                    .OrderByDescending(b =>
                    {
                        if (decimal.TryParse(
                                b.EstimatedYearlySales?
                                    .Replace("USD", "", StringComparison.OrdinalIgnoreCase)
                                    .Replace("$", "")
                                    .Replace(",", "")
                                    .Trim(),
                                out var sales))
                        {
                            return sales;
                        }
                        return 0;
                    })
                    .ToList();
                if (sortedBrands != null)
                    return sortedBrands;
                return  new List<Brand>();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching brands.", ex);
            }
        }
    }
}
