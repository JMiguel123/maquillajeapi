using Microsoft.EntityFrameworkCore;
using MaquillajeApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
//mysql://root:krMhvGSKlsaRLpoChevayhUWcAytaGwp@metro.proxy.rlwy.net:32090/railway
// Add CORS - CONFIGURACIÓN ACTUALIZADA
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Usar CORS - IMPORTANTE: Debe ir antes de UseAuthorization y MapControllers
app.UseCors("AllowReactApp");

app.UseAuthorization();
app.MapControllers();

app.Run();