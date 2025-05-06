using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Services;

namespace ProductApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IProductService _service;
        public BrandController(IProductService service)
        {
            _service = service;
        }

        [HttpGet("{brand_id}")]
        public async Task<IActionResult> GetProductsByBrand(int brand_id)
        {

            var products = await _service.GetProductsByBrand(brand_id);
            return Ok(products);

        }
    }
}
