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
        public long? ProductId { get; set; } // Nullable because in SQL it's not marked as NOT NULL

        [Required]
        [MaxLength(255)]
        [Column("merchant_domain")]
        public string MerchantDomain { get; set; }

        [Required]
        [Column("product_data", TypeName = "json")]
        public JsonDocument ProductData { get; set; }// Using System.Text.Json.Nodes
    }
}
