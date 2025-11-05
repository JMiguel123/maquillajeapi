using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ DIAGNÓSTICO INICIAL
Console.WriteLine("=== DIAGNÓSTICO INICIO ===");
Console.WriteLine($"MYSQL_URL: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MYSQL_URL"))}");
Console.WriteLine($"PORT: {Environment.GetEnvironmentVariable("PORT")}");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

// CONEXIÓN A MySQL - FORZAR MYSQL_URL
var connectionString = Environment.GetEnvironmentVariable("MYSQL_URL");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ MYSQL_URL NO ENCONTRADA - Usando string vacío");
    connectionString = "Server=localhost;Port=3306;Database=test;Uid=root;Pwd=;";
}
else
{
    Console.WriteLine("✅ MYSQL_URL ENCONTRADA");
    Console.WriteLine($"🔗 Connection String: {connectionString.Substring(0, Math.Min(30, connectionString.Length))}...");
}

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

// ✅ CONFIGURAR DbContext SIN appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();

var app = builder.Build();

// ✅ INICIALIZAR BASE DE DATOS CON MANEJO DE ERRORES
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    Console.WriteLine("🔧 Intentando conectar a la base de datos...");
    var canConnect = await context.Database.CanConnectAsync();
    Console.WriteLine($"✅ Conexión a BD: {canConnect}");
    
    if (!canConnect)
    {
        Console.WriteLine("⚠️ No se pudo conectar a la BD, pero continuando...");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error conectando a BD: {ex.Message}");
    // NO salir - dejar que la app inicie sin BD
}

app.UseCors("AllowAll");

// RUTAS
app.MapGet("/", () => new { 
    message = "Maquillaje API funcionando", 
    status = "OK",
    database = "MySQL Railway"
});

app.MapGet("/db-test", async (AppDbContext context) =>
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        return new { 
            database_connected = canConnect,
            message = canConnect ? "✅ BD Conectada" : "❌ BD No conectada"
        };
    }
    catch (Exception ex)
    {
        return new { 
            database_connected = false,
            error = ex.Message
        };
    }
});

app.MapControllers();

// ✅ PUERTO CORRECTO PARA RAILWAY
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
Console.WriteLine($"🚀 INICIANDO APLICACIÓN EN PUERTO: {port}");
app.Run($"http://0.0.0.0:{port}");