//using DatabridgeServer.Data;
//using DatabridgeServer.Services;
//using DatabridgeServer.Services.Products;
//using DatabridgeServer.Services.Students;
//using Microsoft.EntityFrameworkCore;


//var builder = WebApplication.CreateBuilder(args);


//builder.Services.AddControllers();

//// Configure Entity Framework Core with SQL Server
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Register Services


//builder.Services.AddScoped<IProductService, ProductService>();
//builder.Services.AddScoped<IStudentService, StudentService>(); 
//builder.Services.AddScoped<IEmployeeService, EmployeeService>();


//// Add CORS policy
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngular",
//        policy =>
//        {
//            policy
//                .WithOrigins("http://localhost:4200")
//                .AllowAnyHeader()
//                .AllowAnyMethod();
//        });
//});
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//    app.UseSwagger();
//    //app.UseSwaggerUI();
//    //------------------------------
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
//        c.RoutePrefix = string.Empty; // 👈 THIS LINE IS THE KEY
//    });
////------------------------------------
//}

//app.UseHttpsRedirection();
//app.UseCors("AllowAngular");

//app.UseCors("AllowAll");

//app.UseAuthorization();

//app.MapControllers();

//app.Run();


using DatabridgeServer.Data;
using DatabridgeServer.Services;
using DatabridgeServer.Services.Products;
using DatabridgeServer.Services.Students;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// ✅ SINGLE CORS POLICY (Angular)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger (Dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });
}

// ⚠️ ORDER MATTERS
app.UseRouting();

app.UseCors("AllowAngular");   // ✅ CORS HERE

//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
