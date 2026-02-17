using DatabridgeServer.Data;
using DatabridgeServer.Services;
using DatabridgeServer.Services.Employees;
using DatabridgeServer.Services.Members;
using DatabridgeServer.Services.Products;
using DatabridgeServer.Services.Students;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Options;
using OfficeOpenXml;
// Set EPPlus license (REQUIRED for EPPlus 8+)
ExcelPackage.License.SetNonCommercialPersonal("Devaraj");

ExcelPackage.License.SetNonCommercialPersonal("NET");
var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();