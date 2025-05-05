using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;
using ProductApi.Services;
using System.Reflection.Metadata.Ecma335;

namespace ProductApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        public ProductController(IProductService service)
        {
            _service = service;
        }

        [HttpGet("ProductDetails")]
        public async Task<IActionResult> AllProducts()
        {
            var products = await _service.AllProducts();
            return Ok(products);
        }
        [HttpGet("SearchProduct/{search}")]
        public async Task<IActionResult> SearchProduct(string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                var products = await _service.SearchAllProducts(search);
                return Ok(products);
            }

            return BadRequest("Search field is empty");

        }
    }
}
