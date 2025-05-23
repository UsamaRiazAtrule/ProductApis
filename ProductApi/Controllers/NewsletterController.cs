using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers
{
    [Route("api/newsletter")]
    [ApiController]
    public class NewsletterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public NewsletterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }
            // Validate email format
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid email format.");
            }
            // Check if the email already exists in the database
            var existingSubscription = await _context.newsletter_subscriptions
                .FirstOrDefaultAsync(s => s.Email == email);
            if (existingSubscription != null)
            {
                return Conflict("Email is already subscribed.");
            }
            // Create a new subscription
            var subscription = new NewsletterSubscription(email);
            _context.newsletter_subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return Ok("Successfully subscribed to the newsletter.");
        }
    }
}
