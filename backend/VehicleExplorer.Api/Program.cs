
using VehicleExplorer.Api.Clients;
using VehicleExplorer.Api.Middleware;
using VehicleExplorer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. Controllers & API Documentation
// ========================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========================================
// 2. Global Exception Handling
// ========================================

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// ========================================
// 3. Caching & Health Checks
// ========================================

builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();

// ========================================
// 4. External API - NHTSA
// ========================================

builder.Services.AddHttpClient<INhtsaClient, NhtsaClient>(client =>
{
    client.BaseAddress = new Uri(
        "https://vpic.nhtsa.dot.gov/api/vehicles/");

    client.Timeout = TimeSpan.FromSeconds(15);
});

// ========================================
// 5. Application Services
// ========================================

builder.Services.AddScoped<IVehicleCatalog, VehicleCatalogService>();

// ========================================
// 6. CORS
// ========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Frontend:Origin"]
                ?? "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ========================================
// 7. Middleware Pipeline
// ========================================

// Global exception handling
app.UseExceptionHandler();

// Swagger - Development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS
app.UseCors("Frontend");

// ========================================
// 8. Endpoint Mapping
// ========================================

app.MapControllers();
app.MapHealthChecks("/health");

// ========================================
// 9. Run Application
// ========================================

app.Run();

// For WebApplicationFactory integration tests
public partial class Program { }
