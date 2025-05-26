using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models
{
    public class Brand
    {
        public int id { get; set; }
        public string domain { get; set; }
        [Column("estimated_yearly_sales")]
        public string? EstimatedYearlySales { get; set; }
        [Column("estimated_sales_numeric")]
        public decimal? EstimatedYearlyNumeric { get; set; }
        [Column("brand_name")]
        public string? BrandName { get; set; }
        [Column("brand_url")]
        public string? BrandUrl{ get; set; } // the actual domain from master data
        [Column("has_shipping")]
        public bool? HasShipping { get; set; } 
        [Column("has_products")]
        public bool? HasProducts { get; set; }
        [Column("image")]
        public string? Image { get; set; }
    }
    public class BrandDto
    {
        public int id { get; set; }
        [Column("brand_name")]
        public string? BrandName { get; set; }
        [Column("image")]
        public string? Image { get; set; }
    }
}
