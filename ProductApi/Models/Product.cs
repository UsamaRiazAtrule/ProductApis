using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace ProductApi.Models
{
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public long? ProductId { get; set; }  

        [Column("product_url")]
        public string ProductUrl { get; set; } 

        [Required]
        [MaxLength(255)]
        [Column("merchant_domain")]
        public string MerchantDomain { get; set; }

        [Required]
        [Column("product_data", TypeName = "json")]
        public JsonDocument ProductData { get; set; }
    }
}
