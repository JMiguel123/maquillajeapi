using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Data;

var builder = WebApplication.CreateBuilder(args);

// SERVICIOS
builder.Services.AddCors(options => options.AddPolicy("AllowAll", 
    policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// CONFIGURACIÓN DE BD
try
{
    var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
    
    if (!string.IsNullOrEmpty(mysqlUrl))
    {
        var uri = new Uri(mysqlUrl);
        var connectionString = 
            $"Server={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};" +
            $"Uid={uri.UserInfo.Split(':')[0]};Pwd={uri.UserInfo.Split(':')[1]};" +
            "SslMode=Required;AllowPublicKeyRetrieval=true;";
            
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        
        Console.WriteLine($"✅ MySQL configurado: {uri.Host}:{uri.Port}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error MySQL: {ex.Message}");
    
}

builder.Services.AddControllers();

var app = builder.Build();

// ✅ ELIMINAR EnsureCreated() - NO CREAR TABLAS AUTOMÁTICAMENTE
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Solo verificar conexión, NO crear tablas
    var canConnect = await context.Database.CanConnectAsync();
    Console.WriteLine($"📊 Conexión a BD: {canConnect}");
    
    if (canConnect)
    {
        // Verificar si existen datos
        var productCount = await context.Products.CountAsync();
        var saleCount = await context.Sales.CountAsync();
        
        Console.WriteLine($"📦 Productos en BD: {productCount}");
        Console.WriteLine($"💰 Ventas en BD: {saleCount}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Error conectando a BD: {ex.Message}");
}

app.UseCors("AllowAll");
app.MapControllers();

app.MapGet("/", () => new { 
    status = "OK", 
    message = "Maquillaje API funcionando",
    timestamp = DateTime.UtcNow
});



var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");