using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;
using FluentValidation;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables() // Clave para Kubernetes
    .Build();

// 1. Configuración de Logging Estructurado (Consola JSON y Seq)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext() // Permite capturar propiedades dinámicas como requestId
    .WriteTo.Console(new RenderedCompactJsonFormatter()) // Formato estructurado JSON exigido
    .WriteTo.Seq(configuration["Seq:ServerUrl"] ?? "http://seq-service:5341") // Sink HTTP Seq exigido
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

    // 2. Registro automático de Validadores (FluentValidation)
    // Busca todas las clases que hereden de AbstractValidator en el proyecto
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // 3. Configuración de la persistencia con PostgreSQL (Npgsql)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    
    if (!string.IsNullOrEmpty(dbPassword) && !string.IsNullOrEmpty(connectionString))
    {
        connectionString += $";Password={dbPassword}";
    }

    // Nota: Reemplaza 'DbContext' por tu contexto real (ej. 'AppDbContext')
    builder.Services.AddDbContext<DbContext>(options =>
        options.UseNpgsql(connectionString));

    var app = builder.Build();

    // 4. Middleware Obligatorio: Inyección de Propiedad de Correlación (requestId)
    app.Use(async (context, next) =>
    {
        // Genera un ID único para cada petición http
        var requestId = Guid.NewGuid().ToString();
        
        // Empuja la propiedad al contexto de Serilog para que aparezca en Seq
        using (Serilog.Context.LogContext.PushProperty("requestId", requestId))
        {
            context.Response.Headers.Append("X-Request-Id", requestId);
            await next();
        }
    });

    if (app.Environment.IsDevelopment() || true) // Permitir en K8s para pruebas
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
