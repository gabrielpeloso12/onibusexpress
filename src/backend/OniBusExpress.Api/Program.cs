using Microsoft.OpenApi.Models;
using OniBusExpress.Api.Middleware;
using OniBusExpress.Application;
using OniBusExpress.Infra;
using OniBusExpress.Infra.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Registro dos serviços no container de injeção de dependência.
builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAny", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OniBus Express API",
        Version = "v1",
        Description = "API do MVP de vendas de passagens rodoviárias da OniBus Express: consulta de rotas e " +
                      "viagens, criação/consulta/cancelamento de reservas."
    });

    foreach (var assemblyName in new[] { "OniBusExpress.Api", "OniBusExpress.Application" })
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

var app = builder.Build();

// Configuração do pipeline de requisições HTTP.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "OniBus Express API v1");
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAny");

app.MapControllers();

// Os testes de integração sobem seu próprio schema (SQLite in-memory) e seed via
// CustomWebApplicationFactory, então o pipeline real de migration+seed só roda fora do ambiente "Testing".
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OniBusExpressDbContext>();
    await DbInitializer.InitializeAsync(dbContext);
}

app.Run();

/// <summary>Exposta para que <c>WebApplicationFactory&lt;Program&gt;</c> possa inicializar esta API nos testes de integração.</summary>
public partial class Program
{
}
