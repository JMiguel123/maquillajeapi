using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaquillajeApi.Models
{
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("title")]
        public string Title { get; set; } = string.Empty;
        
        [Column("description")]
        public string? Description { get; set; }  // ← Cambiado a nullable
        
        [Column("category")]
        public string? Category { get; set; }     // ← Cambiado a nullable
        
        [Column("price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        
        [Column("discountpercentage", TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; }
        
        [Column("rating", TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; }
        
        [Column("stock")]
        public int Stock { get; set; }
        
        [Column("brand")]
        public string? Brand { get; set; }        // ← Cambiado a nullable
        
        [Column("sku")]
        public string? Sku { get; set; }          // ← Cambiado a nullable
        
        [Column("weight")]
        public int Weight { get; set; }
        
        [Column("dimensions_width", TypeName = "decimal(8,2)")]
        public decimal DimensionsWidth { get; set; }
        
        [Column("dimensions_height", TypeName = "decimal(8,2)")]
        public decimal DimensionsHeight { get; set; }
        
        [Column("dimensions_depth", TypeName = "decimal(8,2)")]
        public decimal DimensionsDepth { get; set; }
        
        [Column("warrantyinformation")]
        public string? WarrantyInformation { get; set; }  // ← Cambiado a nullable
        
        [Column("shippinginformation")]
        public string? ShippingInformation { get; set; }  // ← Cambiado a nullable
        
        [Column("availabilitystatus")]
        public string? AvailabilityStatus { get; set; }   // ← Cambiado a nullable
        
        [Column("returnpolicy")]
        public string? ReturnPolicy { get; set; }         // ← Cambiado a nullable
        
        [Column("minimumorderquantity")]
        public int MinimumOrderQuantity { get; set; }
        
        [Column("thumbnail")]
        public string? Thumbnail { get; set; }            // ← Cambiado a nullable
        
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
        
        // Navigation properties
        public ICollection<ProductTag> Tags { get; set; } = new List<ProductTag>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }

    [Table("product_tags")]
    public class ProductTag
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("product_id")]
        public int ProductId { get; set; }
        
        [Column("tag")]
        public string Tag { get; set; } = string.Empty;
        
        // Navigation property
        public Product Product { get; set; } = null!;
    }

    [Table("product_reviews")]
    public class ProductReview
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("product_id")]
        public int ProductId { get; set; }
        
        [Column("rating")]
        public int Rating { get; set; }
        
        [Column("comment")]
        public string? Comment { get; set; }              // ← Cambiado a nullable
        
        [Column("date")]
        public DateTime Date { get; set; }
        
        [Column("reviewername")]
        public string ReviewerName { get; set; } = string.Empty;
        
        [Column("revieweremail")]
        public string ReviewerEmail { get; set; } = string.Empty;
        
        // Navigation property
        public Product Product { get; set; } = null!;
    }

    [Table("sales")]
    public class Sale
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("product_title")]
        public string ProductTitle { get; set; } = string.Empty;
        
        [Column("product_description")]
        public string ProductDescription { get; set; } = string.Empty;
        
        [Column("product_price", TypeName = "decimal(10,2)")]
        public decimal ProductPrice { get; set; }
        
        [Column("product_sku")]
        public string ProductSku { get; set; } = string.Empty;
        
        [Column("product_tag")]
        public string ProductTag { get; set; } = string.Empty;
        
        [Column("sale_date")]
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        
        // Podemos mantener el ProductId como referencia opcional
        [Column("product_id")]
        public int? ProductId { get; set; }
        
        // Navigation property (opcional)
        public Product? Product { get; set; }
    }
}