using Microsoft.EntityFrameworkCore;
using ProductApi.Models;
using System;

namespace ProductApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Product> products { get; set; }
        public DbSet<Brand> merchants { get; set; }

        public DbSet<ProductDetails> productDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<System.Text.Json.Nodes.JsonNode>();
            modelBuilder.Ignore<System.Text.Json.Nodes.JsonObject>();
            modelBuilder.Ignore<System.Text.Json.Nodes.JsonArray>();
        }

    }
}
