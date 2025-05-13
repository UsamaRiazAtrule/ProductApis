using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models
{
    public class Brand
    {
        public int id { get; set; }
        public string domain { get; set; }
        [Column("estimated_yearly_sales")]
        public string EstimatedYearlySales { get; set; }
        [Column("brand_name")]
        public string? BrandName { get; set; }
        public string? image { get; set; }
    }
}
