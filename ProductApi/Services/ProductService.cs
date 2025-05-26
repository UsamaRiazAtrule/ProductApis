using Microsoft.AspNetCore.Routing.Constraints;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
                                             AND  (product_data->>'price')::numeric > 0 
                                             AND (product_data->>'available')::boolean = true
                                             AND is_food_or_drink = true
                                             ").ToListAsync();
                    totalCount = result.FirstOrDefault();


                    var baseSearchQuery = @"SELECT * FROM products WHERE search_vector @@ to_tsquery('english', {0})
                       AND (product_data->>'price')::numeric > 0 
                       AND (product_data->>'available')::boolean = true
                          AND is_food_or_drink = true   
                       ";
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
                    string baseQuery = @"SELECT * FROM products 
                        WHERE (product_data->>'price')::numeric > 0 
                        AND (product_data->>'available')::boolean = true 
                        AND is_food_or_drink = true ";
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
                        orderClause = " ORDER BY id ";
                    }

                    var fullQuery = baseQuery + orderClause + "LIMIT {0} OFFSET {1}";


                    var countResult = await _context.Database.SqlQuery<int>($@"
                                             SELECT COUNT(id)
                                             FROM products 
                                             WHERE (product_data->>'price')::numeric > 0 
                                             AND (product_data->>'available')::boolean = true
                                             AND is_food_or_drink = true 
                                             ").ToListAsync();
                    totalCount = countResult.FirstOrDefault();

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
                    ProductUrl = $"{product.MerchantDomain}{product.ProductUrl}",
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
                var merchant = await _context.merchants
                    .Where(m => m.id == brand_id)
                    .Select(m => new { m.domain, m.BrandName, m.BrandUrl, m.Image })
                    .FirstOrDefaultAsync();

                var brandDescription = string.Empty;
                if (!string.IsNullOrWhiteSpace(merchant?.BrandUrl))
                {

                    var brandDescriptionResult = await _context.Database
                        .SqlQueryRaw<string>(@"SELECT COALESCE(meta_description, aboutus_text) AS description
                            FROM public.meta_og_scraped_content
                            WHERE domainname =  {0}", merchant.BrandUrl).ToListAsync();

                    brandDescription = brandDescriptionResult.FirstOrDefault();
                }
                if (string.IsNullOrEmpty(merchant?.domain))
                    return new PaginatedProductDto();

                // Build ORDER BY clause
                string orderBy;
                if (!string.IsNullOrWhiteSpace(sortByPrice) &&
                    (sortByPrice.Equals("LTH", StringComparison.OrdinalIgnoreCase) ||
                     sortByPrice.Equals("HTL", StringComparison.OrdinalIgnoreCase)))
                {
                    var dir = sortByPrice.Equals("LTH", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
                    orderBy = @$"(product_data->>'price')::numeric {dir}";
                }
                else
                {
                    // fallback ordering (by Id here; change as needed)
                    orderBy = "id ASC";
                }

                // Compute offset
                var offset = (pageNumber - 1) * pageSize;

                // Fetch paged products via a single RAW SQL query
                var sql = $@"
                            SELECT *
                            FROM products
                            WHERE merchant_domain = {{0}}
                              AND (product_data->>'price')::numeric > 0
                              AND (product_data->>'available')::boolean = true
                               AND is_food_or_drink = true
                            ORDER BY {orderBy}
                            OFFSET {{1}}
                            LIMIT {{2}}";

                var products = await _context.products
                    .FromSqlRaw(sql, merchant?.domain, offset, pageSize)
                    .ToListAsync();

                //var totalCount = await _context.Database
                //                        .SqlQuery<int>(@$"SELECT COUNT(*) FROM products
                //                         WHERE  merchant_domain = {domain} AND
                //                        (product_data->>'price')::numeric > 0")
                //                        .SingleAsync();

                //var countQuery = _context.products
                //.FromSqlRaw(@"
                //    SELECT id
                //    FROM products
                //    WHERE merchant_domain = {0}
                //      AND (product_data->>'price')::numeric > 0
                //      AND (product_data->>'available')::boolean = true", merchant?.domain);

                //var totalCount = await countQuery.CountAsync();

                var countResult = await _context.Database.SqlQueryRaw<int>($@"
                                             SELECT count(id)
                                                FROM products
                                                WHERE merchant_domain = {{0}}
                                                  AND (product_data->>'price')::numeric > 0
                                                  AND (product_data->>'available')::boolean = true
                                                  AND is_food_or_drink = true
                                             ", merchant?.domain).ToListAsync();
                var totalCount = countResult.FirstOrDefault();

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
                    Products = productDto,
                    BrandDescription = brandDescription,
                    BrandImage = merchant?.Image,
                    BrandName = merchant?.BrandName
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

        public async Task<List<BrandDto>> Brands(int page, int size)
        {
            try
            {
                //var brands = await _context.merchants
                //    .Where(b=>b.HasShipping==true)
                //    .ToListAsync();
                //var sortedBrands = brands
                //    .OrderByDescending(b =>
                //    {
                //        if (decimal.TryParse(
                //                b.EstimatedYearlySales?
                //                    .Replace("USD", "", StringComparison.OrdinalIgnoreCase)
                //                    .Replace("$", "")
                //                    .Replace(",", "")
                //                    .Trim(),
                //                out var sales))
                //        {
                //            return sales;
                //        }
                //        return 0;
                //    })
                //    .ToList();

                //var sortedBrands = await _context.merchants
                //    .FromSqlRaw(@"
                //        SELECT * FROM merchants
                //        WHERE has_shipping = true
                //        ORDER BY 
                //            COALESCE(
                //                CAST(
                //                    REPLACE(
                //                        REPLACE(
                //                            REPLACE(estimated_yearly_sales, 'USD', ''),
                //                            '$', ''),
                //                        ',', '') AS DECIMAL), 0) DESC
                //    ")
                //    .ToListAsync();

                //var sortedBrands = await _context.merchants
                //    .FromSqlRaw(@"
                //       select *
                //        from merchants 
                //        where has_shipping = true
                //        and has_products = true
                //        order by coalesce(estimated_sales_numeric, 0) desc
                //        limit 20 offset 0
                //        ;
                //    ")
                //    .ToListAsync();

                //if (sortedBrands != null)
                //    return sortedBrands;
                //return  new List<Brand>();

                var offset = (page - 1) * size;

                var brands = await _context.merchants
                    .Where(m => m.HasShipping == true && m.HasProducts == true)
                    .OrderByDescending(m => m.EstimatedYearlyNumeric)
                    .Skip(offset)
                    .Take(size)
                    .Select(m => new BrandDto
                    {
                        id = m.id,
                        BrandName = m.BrandName,
                        Image = m.Image
                    })
                    .ToListAsync();

                return brands;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching brands.", ex);
            }
        }
    }
}
