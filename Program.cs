using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ DIAGNÓSTICO
Console.WriteLine("=== CONFIGURACIÓN ===");
Console.WriteLine($"MYSQL_URL: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MYSQL_URL"))}");
Console.WriteLine($"PORT: {Environment.GetEnvironmentVariable("PORT")}");

// ✅ CONVERTIR MYSQL_URL
string connectionString = GetMySqlConnectionString();
Console.WriteLine($"🔗 Connection String: {connectionString.Replace("Pwd=", "Pwd=***")}");

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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();

var app = builder.Build();

// ✅ INICIALIZAR BD
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var canConnect = await context.Database.CanConnectAsync();
    Console.WriteLine($"📊 Conexión BD: {(canConnect ? "✅ CONECTADA" : "❌ FALLIDA")}");
}
catch (Exception ex)
{
    Console.WriteLine($"💥 Error BD: {ex.Message}");
}

app.UseCors("AllowAll");

// ✅ RUTAS SIMPLES
app.MapGet("/", () => new { 
    message = "Maquillaje API funcionando", 
    status = "OK",
    timestamp = DateTime.UtcNow
});

app.MapControllers();

// ✅ INICIAR
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
Console.WriteLine($"🚀 Iniciando en puerto: {port}");
app.Run($"http://0.0.0.0:{port}");

// ✅ CONVERSIÓN MYSQL_URL
static string GetMySqlConnectionString()
{
    var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
    
    if (string.IsNullOrEmpty(mysqlUrl))
        return "Server=localhost;Port=3306;Database=test;Uid=root;Pwd=;";

    try
    {
        var uri = new Uri(mysqlUrl);
        return $"Server={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Uid={uri.UserInfo.Split(':')[0]};Pwd={uri.UserInfo.Split(':')[1]};";
    }
    catch
    {
        return mysqlUrl;
    }
}