using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables() // Clave para Kubernetes
    .Build();

// 1. Configuración de Logging Estructurado (JSON para stdout/Kubernetes)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.Console(new RenderedCompactJsonFormatter()) // Formato estructurado JSON
    .CreateLogger();

try
{
    Log.Information("Iniciando la API del Bootcamp...");

    var builder = WebApplication.CreateBuilder(args);

    // Reemplazar el logger por defecto con Serilog
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // 2. Configuración de la persistencia con PostgreSQL (Npgsql)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    // Nota de seguridad: En Kubernetes, la contraseña se inyectará dinámicamente
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    
    if (!string.IsNullOrEmpty(dbPassword) && !string.IsNullOrEmpty(connectionString))
    {
        connectionString += $";Password={dbPassword}";
    }

    builder.Services.AddDbContext<DbContext>(options =>
        options.UseNpgsql(connectionString));

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();
    app.MapControllers();

    Log.Information("API corriendo exitosamente.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La API terminó inesperadamente debido a un error crítico.");
}
finally
{
    Log.CloseAndFlush();
}
