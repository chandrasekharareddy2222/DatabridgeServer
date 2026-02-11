using Microsoft.EntityFrameworkCore;
using DatabridgeServer.Data;
using DatabridgeServer.Services.Products;
using DatabridgeServer.Services.Students;
using DatabridgeServer.Swagger;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// EPPlus License (REQUIRED)
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
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

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
// Swagger (Swashbuckle ONLY)
// ===============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<FileUploadOperationFilter>();
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
