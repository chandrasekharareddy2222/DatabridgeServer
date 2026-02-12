using DatabridgeServer.Data;
using DatabridgeServer.Services;
using DatabridgeServer.Services.Employees;
using DatabridgeServer.Services.Products;
using DatabridgeServer.Services.Students;
using DatabridgeServer.Services.Members;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

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

builder.Services.AddOpenApi();

// ===============================
// Swagger / OpenAPI
// ===============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
