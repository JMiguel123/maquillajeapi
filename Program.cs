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

// **CONEXIÓN CON FALLBACKS MÚLTIPLES**
string connectionString = GetConnectionString();

Console.WriteLine($"✅ Conectando a MySQL...");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAll");
app.MapControllers();
app.Run();

// **MÉTODO PARA OBTENER CADENA DE CONEXIÓN**
static string GetConnectionString()
{
    // 1. Primero intenta con MYSQL_URL (Railway)
    var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
    if (!string.IsNullOrEmpty(mysqlUrl))
    {
        Console.WriteLine("🔗 Usando MYSQL_URL");
        return mysqlUrl;
    }

    // 2. Si no, construye con variables individuales
    var host = Environment.GetEnvironmentVariable("MYSQLHOST") ?? "localhost";
    var port = Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306";
    var database = Environment.GetEnvironmentVariable("MYSQLDATABASE") ?? "maquillaje_db";
    var user = Environment.GetEnvironmentVariable("MYSQLUSER") ?? "root";
    var password = Environment.GetEnvironmentVariable("MYSQLPASSWORD") ?? "password";

    // 3. Si no hay variables de entorno, usa appsettings.json
    if (host == "localhost" && user == "root")
    {
        // Esto cargará desde appsettings.json
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(defaultConnection))
        {
            Console.WriteLine("📁 Usando appsettings.json");
            return defaultConnection;
        }
    }

    Console.WriteLine("🏗️ Construyendo connection string desde variables de entorno");
    return $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";
}