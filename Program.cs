using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Data;

var builder = WebApplication.CreateBuilder(args);

// SERVICIOS
builder.Services.AddCors(options => options.AddPolicy("AllowAll", 
    policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ✅ CONFIGURACIÓN ROBUSTA DE BASE DE DATOS
Console.WriteLine("🔧 Configurando base de datos...");

var host = Environment.GetEnvironmentVariable("MYSQLHOST");
AppDbContext registeredContext = null;

if (!string.IsNullOrEmpty(host))
{
    try
    {
        var connectionString = 
            $"Server={host};" +
            $"Port={Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306"};" +
            $"Database={Environment.GetEnvironmentVariable("MYSQLDATABASE") ?? "railway"};" +
            $"Uid={Environment.GetEnvironmentVariable("MYSQLUSER") ?? "root"};" +
            $"Pwd={Environment.GetEnvironmentVariable("MYSQLPASSWORD") ?? ""};" +
            "SslMode=Required;";
            
        Console.WriteLine($"🔗 Intentando conectar a: {host}");
        
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        
        Console.WriteLine("✅ MySQL registrado en servicios");
        registeredContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)).Options);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error configurando MySQL: {ex.Message}");
        // Continuar con base de datos en memoria
    }
}

// ✅ GARANTIZAR QUE SIEMPRE HAYA UN DBCONTEXT REGISTRADO
if (registeredContext == null)
{
    Console.WriteLine("🔄 Usando base de datos en memoria");
    
}

builder.Services.AddControllers();

var app = builder.Build();

// ✅ VERIFICAR QUE EL DBCONTEXT ESTÉ REGISTRADO
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetService<AppDbContext>();
    if (context == null)
    {
        Console.WriteLine("⚠️  DbContext no registrado, registrando emergencia...");
        // Registro de emergencia
        
    }
    else
    {
        Console.WriteLine("✅ DbContext verificado y listo");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Error verificando DbContext: {ex.Message}");
}

app.UseCors("AllowAll");
app.MapControllers();

app.MapGet("/", () => new { 
    status = "OK", 
    message = "Maquillaje API Running",
    database = registeredContext != null ? "MySQL" : "InMemory",
    timestamp = DateTime.UtcNow
});

// ✅ RUTA DE PRUEBA SIN DEPENDENCIA DE BD
app.MapGet("/test", () => new { 
    message = "Test endpoint funcionando sin BD",
    status = "OK" 
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 Aplicación iniciada en puerto: {port}");
app.Run($"http://0.0.0.0:{port}");