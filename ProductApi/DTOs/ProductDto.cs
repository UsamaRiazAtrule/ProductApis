using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ProductApi.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string ProductUrl { get; set; }
        public JsonDocument ProductData { get; set; }
    }
}
