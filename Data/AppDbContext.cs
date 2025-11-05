using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Models;

namespace MaquillajeApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar nombres EXACTOS de tabla
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<ProductTag>().ToTable("product_tags");  // ← Este es el importante
            modelBuilder.Entity<ProductReview>().ToTable("product_reviews"); // ← Y este
            modelBuilder.Entity<Sale>().ToTable("sales");

            // Configurar relaciones
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Tags)
                .WithOne(pt => pt.Product)
                .HasForeignKey(pt => pt.ProductId);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(pr => pr.Product)
                .HasForeignKey(pr => pr.ProductId);

             modelBuilder.Entity<Sale>()
        .HasOne(s => s.Product)
        .WithMany()
        .HasForeignKey(s => s.ProductId)
        .IsRequired(false); // Hacer la relación opcional

            // Configurar claves primarias si es necesario
            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);
                
            modelBuilder.Entity<ProductTag>()
                .HasKey(pt => pt.Id);
                
            modelBuilder.Entity<ProductReview>()
                .HasKey(pr => pr.Id);
                
            modelBuilder.Entity<Sale>()
                .HasKey(s => s.Id);
        }
    }
}