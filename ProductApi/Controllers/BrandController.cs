﻿using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> Brands()
        {
            var brands = await _service.Brands();
            return Ok(brands);
        }
    }
}
