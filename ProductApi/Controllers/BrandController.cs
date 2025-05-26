using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Services;

namespace ProductApi.Controllers
{
    [Route("api/brand")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IProductService _service;
        public BrandController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Brands(int page = 1, int size = 30)
        {
            var brands = await _service.Brands(page, size);
            return Ok(brands);
        }
    }
}
