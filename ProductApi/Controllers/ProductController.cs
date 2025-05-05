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

        [HttpGet("AllProducts")]
        public async Task<IActionResult> AllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
        {
            
                var products = await _service.AllProducts(pageNumber, pageSize, search);
                return Ok(products);

        }
        [HttpGet("SearchProductById/{id}")]
        public async Task<IActionResult> SearchProductById(int id)
        {
            if (id > 0)
            {
                var product = await _service.SearchProductById(id);
                return Ok(product); 
            }

            return BadRequest("Invalid product ID.");
        }

    }
}
