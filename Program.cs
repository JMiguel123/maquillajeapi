using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Data;

var builder = WebApplication.CreateBuilder(args);

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
    Console.WriteLine($"🔗 String de conexión obtenida: {!string.IsNullOrEmpty(connectionString)}");
    
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error en conexión MySQL: {ex.Message}");
}

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAll");

// ✅ RUTAS ESENCIALES
app.MapGet("/", () => Results.Json(new { 
    message = "Maquillaje API funcionando", 
    status = "OK",
    timestamp = DateTime.UtcNow 
}));

app.MapGet("/health", () => Results.Json(new { 
    status = "Healthy",
    database = "Connected" 
}));

app.MapControllers();

// ✅ PUERTO DE RAILWAY
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
Console.WriteLine($"🚀 Iniciando aplicación en puerto: {port}");
app.Run($"http://0.0.0.0:{port}");