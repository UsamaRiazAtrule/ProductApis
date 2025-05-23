using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductApi.Models
{
    public class NewsletterSubscription
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("subscribed_at")]
        public DateTime SubscribedAt { get; set; }
        public NewsletterSubscription(string email)
        {
            Email = email;
            SubscribedAt = DateTime.UtcNow;
        }
    }
}
