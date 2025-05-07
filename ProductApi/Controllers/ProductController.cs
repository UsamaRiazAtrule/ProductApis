using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;
using ProductApi.Services;
using System.Reflection.Metadata.Ecma335;

namespace ProductApi.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        public ProductController(IProductService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "", [FromQuery] string sortByPriceDirection = "")
        {
            
                var products = await _service.AllProducts(pageNumber, pageSize, search, sortByPriceDirection);
                return Ok(products);

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            if (id > 0)
            {
                var product = await _service.SearchProductById(id);
                return Ok(product); 
            }

            return BadRequest("Invalid product ID.");
        }

        [HttpGet("brand/{id}")]
        public async Task<IActionResult> GetProductsByBrand(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {

            var products = await _service.GetProductsByBrand(id, pageNumber, pageSize);
            return Ok(products);

        }
    }
}
