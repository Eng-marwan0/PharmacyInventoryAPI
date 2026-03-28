using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PharmacyInventoryAPI.Data;
using PharmacyInventoryAPI.Middleware;
using PharmacyInventoryAPI.Services;
using PharmacyInventoryAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<PharmacyDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=pharmacy.db"));

// Services
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IBatchService, BatchService>();
builder.Services.AddScoped<IExpiryService, ExpiryService>();

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateMedicationValidator>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pharmacy Inventory API",
        Version = "v1",
        Description = "API for managing pharmacy inventory with batch tracking and expiry handling.",
        Contact = new OpenApiContact
        {
            Name = "Pharmacy Inventory Support"
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();
    db.Database.EnsureCreated();
}

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger (always enabled)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pharmacy Inventory API v1");
    options.RoutePrefix = string.Empty;
});

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
