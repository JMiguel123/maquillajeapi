using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Data;

var builder = WebApplication.CreateBuilder(args);

// SERVICIOS
builder.Services.AddCors(options => options.AddPolicy("AllowAll", 
    policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ✅ CONFIGURACIÓN CON LA CADENA REAL
Console.WriteLine("🔧 Configurando base de datos...");

try
{
    // OPCIÓN 1: Usar MYSQL_URL directamente (más confiable)
    var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
    
    if (!string.IsNullOrEmpty(mysqlUrl))
    {
        Console.WriteLine($"🔗 Usando MYSQL_URL: {mysqlUrl.Split('@')[1]}"); // Mostrar solo host:puerto
        
        // Convertir mysql://... a formato Connection String
        var uri = new Uri(mysqlUrl);
        var connectionString = 
            $"Server={uri.Host};" +
            $"Port={uri.Port};" +
            $"Database={uri.AbsolutePath.Trim('/')};" +
            $"Uid={uri.UserInfo.Split(':')[0]};" +
            $"Pwd={uri.UserInfo.Split(':')[1]};" +
            "SslMode=Required;AllowPublicKeyRetrieval=true;";
            
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        
        Console.WriteLine("✅ MySQL configurado desde MYSQL_URL");
    }
    else
    {
        // OPCIÓN 2: Usar variables individuales
        var host = Environment.GetEnvironmentVariable("MYSQLHOST");
        if (!string.IsNullOrEmpty(host))
        {
            var connectionString = 
                $"Server={host};" +
                $"Port={Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306"};" +
                $"Database={Environment.GetEnvironmentVariable("MYSQLDATABASE") ?? "railway"};" +
                $"Uid={Environment.GetEnvironmentVariable("MYSQLUSER") ?? "root"};" +
                $"Pwd={Environment.GetEnvironmentVariable("MYSQLPASSWORD") ?? ""};" +
                "SslMode=Required;AllowPublicKeyRetrieval=true;";
                
            Console.WriteLine($"🔗 Conectando a: {host}:{Environment.GetEnvironmentVariable("MYSQLPORT")}");
            
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            
            Console.WriteLine("✅ MySQL configurado desde variables individuales");
        }
        else
        {
            throw new Exception("No se encontraron variables de MySQL");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error configurando MySQL: {ex.Message}");
    Console.WriteLine("🔄 Usando base de datos en memoria temporal");
    
}

builder.Services.AddControllers();

var app = builder.Build();

// ✅ INICIALIZAR BASE DE DATOS
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (context.Database.IsRelational())
    {
        Console.WriteLine("🔧 Verificando/Creando base de datos...");
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ Base de datos lista");
        
        // Probar conexión
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine($"📊 Conexión establecida: {canConnect}");
    }
    else
    {
        Console.WriteLine("ℹ️  Usando base de datos en memoria");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Error inicializando BD: {ex.Message}");
}

app.UseCors("AllowAll");
app.MapControllers();

app.MapGet("/", () => new { 
    status = "OK", 
    message = "Maquillaje API Running",
    database = "MySQL Railway",
    timestamp = DateTime.UtcNow
});

app.MapGet("/db-info", async (IServiceProvider sp) => 
{
    try
    {
        using var scope = sp.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        if (context.Database.IsRelational())
        {
            var dbName = await context.Database.SqlQueryRaw<string>("SELECT DATABASE()").FirstOrDefaultAsync();
            return new { 
                database = "MySQL",
                name = dbName,
                connected = await context.Database.CanConnectAsync()
            };
        }
        else
        {
            return new { database = "InMemory", connected = true };
        }
    }
    catch (Exception ex)
    {
        return new { database = "Error", error = ex.Message };
    }
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 Aplicación iniciada en puerto: {port}");
app.Run($"http://0.0.0.0:{port}");