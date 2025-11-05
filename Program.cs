using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ DIAGNÓSTICO DE PUERTOS
Console.WriteLine("=== CONFIGURACIÓN DE PUERTOS ===");
Console.WriteLine($"PORT: {Environment.GetEnvironmentVariable("PORT")}");
Console.WriteLine($"HTTP_PORTS: {Environment.GetEnvironmentVariable("HTTP_PORTS")}");
Console.WriteLine($"HTTPS_PORTS: {Environment.GetEnvironmentVariable("HTTPS_PORTS")}");
Console.WriteLine($"ASPNETCORE_URLS: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");

// CONFIGURAR SERVICIOS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ✅ CONFIGURACIÓN MÍNIMA DE BD TEMPORAL
try
{
    var host = Environment.GetEnvironmentVariable("MYSQLHOST");
    var ports = Environment.GetEnvironmentVariable("MYSQLPORT");
    var database = Environment.GetEnvironmentVariable("MYSQLDATABASE");
    var user = Environment.GetEnvironmentVariable("MYSQLUSER");
    var password = Environment.GetEnvironmentVariable("MYSQLPASSWORD");

    if (!string.IsNullOrEmpty(host))
    {
        var connectionString = $"Server={host};Port={ports};Database={database};Uid={user};Pwd={password};SslMode=Required;";
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        Console.WriteLine("✅ MySQL configurado");
    }
    else
    {
        Console.WriteLine("⚠️  Usando base de datos en memoria");
        
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error configurando BD: {ex.Message}");
    
}

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAll");

// RUTAS BÁSICAS
app.MapGet("/", () => new { 
    message = "Maquillaje API funcionando", 
    status = "OK",
    timestamp = DateTime.UtcNow
});

app.MapControllers();

// ✅ SOLUCIÓN DEFINITIVA PARA PUERTOS
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 INICIANDO EN PUERTO: {port}");

// ✅ FORZAR LA CONFIGURACIÓN DEL PUERTO
app.Run($"http://0.0.0.0:{port}");