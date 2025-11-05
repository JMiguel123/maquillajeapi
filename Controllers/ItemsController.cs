using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Data;
using MaquillajeApi.Models;

namespace MaquillajeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/items?q=:query
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProducts([FromQuery] string q = "")
        {
            var query = _context.Products
                .Include(p => p.Tags)
                .Include(p => p.Reviews)
                .AsQueryable();

            // Filtrar por término de búsqueda
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(p => 
                    p.Title.Contains(q) || 
                    p.Description.Contains(q) || 
                    p.Category.Contains(q) ||
                    p.Tags.Any(t => t.Tag.Contains(q)) ||
                    p.Brand.Contains(q)
                );
            }

            var products = await query
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    p.Category,
                    p.Price,
                    p.DiscountPercentage,
                    p.Rating,
                    p.Stock,
                    p.Brand,
                    p.Thumbnail,
                    p.AvailabilityStatus,
                    Tags = p.Tags.Select(t => t.Tag).ToList(),
                    ReviewCount = p.Reviews.Count,
                    AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Tags)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var result = new
            {
                product.Id,
                product.Title,
                product.Description,
                product.Category,
                product.Price,
                product.DiscountPercentage,
                product.Rating,
                product.Stock,
                product.Brand,
                product.Sku,
                product.Weight,
                Dimensions = new
                {
                    product.DimensionsWidth,
                    product.DimensionsHeight,
                    product.DimensionsDepth
                },
                product.WarrantyInformation,
                product.ShippingInformation,
                product.AvailabilityStatus,
                product.ReturnPolicy,
                product.MinimumOrderQuantity,
                product.Thumbnail,
                Tags = product.Tags.Select(t => t.Tag).ToList(),
                Reviews = product.Reviews.Select(r => new
                {
                    r.Rating,
                    r.Comment,
                    r.Date,
                    r.ReviewerName,
                    r.ReviewerEmail
                }).ToList()
            };

            return Ok(result);
        }

        // POST: api/addSale
[HttpPost("addSale")]
public async Task<ActionResult<bool>> AddSale([FromBody] SaleRequest request)
{
    try
    {
        // Obtener el producto completo
        var product = await _context.Products
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);
            
        if (product == null)
        {
            return BadRequest(new { success = false, message = "Producto no encontrado" });
        }

        // Obtener el primer tag del producto
        var mainTag = product.Tags.FirstOrDefault()?.Tag ?? "general";

        // Crear la venta con los nuevos campos
        var sale = new Sale
        {
            ProductTitle = product.Title ?? "Producto sin nombre",
            ProductDescription = product.Description ?? "Sin descripción",
            ProductPrice = product.Price,
            ProductSku = product.Sku ?? "N/A",
            ProductTag = mainTag,
            SaleDate = DateTime.Now,
            ProductId = product.Id
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Venta registrada exitosamente" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al agregar venta: {ex.Message}");
        return StatusCode(500, new { success = false, message = "Error interno del servidor" });
    }
}

// GET: api/sales (actualizado)
[HttpGet("sales")]
public async Task<ActionResult<IEnumerable<object>>> GetSales()
{
    try
    {
        var sales = await _context.Sales
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new
            {
                s.Id,
                s.ProductTitle,
                s.ProductDescription,
                s.ProductPrice,
                s.ProductSku,
                s.ProductTag,
                SaleDate = s.SaleDate.ToString("yyyy-MM-ddTHH:mm:ss")
            })
            .ToListAsync();

        return Ok(sales);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al obtener ventas: {ex.Message}");
        return StatusCode(500, new { error = "Error al obtener las ventas" });
    }
}
    }

    // DTO para recibir datos de venta
    public class SaleRequest
{
    public int ProductId { get; set; }
}
}