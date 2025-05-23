using Microsoft.EntityFrameworkCore;
using ProductApi.DTOs;
using ProductApi.Models;
using System;

namespace ProductApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Product> products { get; set; }
        public DbSet<Brand> merchants { get; set; }
        public DbSet<NewsletterSubscription> newsletter_subscriptions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<System.Text.Json.Nodes.JsonNode>();
            modelBuilder.Ignore<System.Text.Json.Nodes.JsonObject>();
            modelBuilder.Ignore<System.Text.Json.Nodes.JsonArray>();

            // Ignore Product and Brand by mapping to no table, as they are for readonly
            //modelBuilder.Entity<Product>().ToTable((string)null);
            //modelBuilder.Entity<Brand>().ToTable((string)null);

            //base.OnModelCreating(modelBuilder);
        }

        // Exclude the entities from Migrations (to not be tracked in model snapshot)
        //public override int SaveChanges()
        //{
        //    var entries = ChangeTracker.Entries().Where(e => e.Entity is Product || e.Entity is Brand);
        //    foreach (var entry in entries)
        //    {
        //        entry.State = EntityState.Detached;
        //    }
        //    return base.SaveChanges();
        //}

    }
}
