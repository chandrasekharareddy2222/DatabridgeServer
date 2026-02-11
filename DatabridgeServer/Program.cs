    
using DatabridgeServer.Data;
using DatabridgeServer.Services.Products;
using DatabridgeServer.Services.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// EPPlus License
// ===============================
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// ===============================
// Controllers
// ===============================
builder.Services.AddControllers();

// ===============================
// Entity Framework Core
// ===============================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===============================
// Dependency Injection
// ===============================
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStudentService, StudentService>();

// ===============================
// CORS
// ===============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===============================
// Swagger / OpenAPI
// ===============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DatabridgeServer API",
        Version = "v1",
        Description = "API endpoints for Students and Products"
    });
});

var app = builder.Build();

// ===============================
// Middleware Pipeline
// ===============================
if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
