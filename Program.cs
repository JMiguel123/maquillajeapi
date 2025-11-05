using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Al inicio de Program.cs, después de builder
Console.WriteLine("=== DIAGNÓSTICO INICIO ===");
Console.WriteLine($"MYSQL_URL: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MYSQL_URL"))}");
Console.WriteLine($"PORT: {Environment.GetEnvironmentVariable("PORT")}");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
Console.WriteLine("=== DIAGNÓSTICO FIN ===");

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Conexión a MySQL
try
{
    var connectionString = Environment.GetEnvironmentVariable("MYSQL_URL");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("⚠️ MYSQL_URL no encontrada");
        connectionString = "Server=localhost;Port=3306;Database=test;Uid=root;Pwd=password;";
    }
    else
    {
        Console.WriteLine("🔗 MYSQL_URL encontrada");
    }
    
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        
    Console.WriteLine("✅ MySQL configurado");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERROR MySQL: {ex.Message}");
}

builder.Services.AddControllers();

var app = builder.Build();

// ✅ INICIALIZAR BASE DE DATOS
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated(); // Crea las tablas si no existen
        Console.WriteLine("✅ Base de datos inicializada");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error inicializando BD: {ex.Message}");
    }
}

app.UseCors("AllowAll");

// Ruta raíz
app.MapGet("/", () => new { 
    message = "Maquillaje API funcionando", 
    status = "OK",
    database = "MySQL"
});

app.MapControllers();

// ✅ USAR PUERTO DE RAILWAY - ESTO ES CLAVE
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
Console.WriteLine($"🚀 Iniciando en puerto: {port}");
app.Run($"http://0.0.0.0:{port}");