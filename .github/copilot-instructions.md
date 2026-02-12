# GitHub Copilot Instructions for DatabridgeServer

## Project Overview
DatabridgeServer is a .NET 10.0 Web API project with a modular architecture. The project follows clean architecture principles with separation of concerns between Controllers, Services, and Data layers.

## Architecture Pattern

### Layered Architecture
- **Controllers**: Handle HTTP requests/responses only. No business logic.
- **Services**: Contains all business logic and validation
- **Models**: Entity definitions with data annotations
- **Data**: Entity Framework Core DbContext and database operations

### Modular Structure
Each feature module (Products, Users, Brands, etc.) should follow this structure:
```
Controllers/
  └── [ModuleName]/
      └── [ModuleName]Controller.cs
Services/
  └── [ModuleName]/
      ├── I[ModuleName]Service.cs
      └── [ModuleName]Service.cs
Models/
  └── [ModelName].cs
```

## Coding Guidelines

### Controller Development
- Controllers should be thin and only handle HTTP concerns
- Always inject services via constructor (Dependency Injection)
- Use appropriate HTTP status codes (200, 201, 204, 400, 404, etc.)
- Return descriptive error messages in responses
- Use `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` attributes
- Namespace should be `DatabridgeServer.Controllers.[ModuleName]`

Example:
```csharp
namespace DatabridgeServer.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
    }
}
```

### Service Development
- All business logic goes in services
- Create an interface for each service (I[Name]Service)
- Services handle database operations, validation, and business rules
- Use async/await for all database operations
- Return domain objects or boolean for success/failure
- Namespace should be `DatabridgeServer.Services.[ModuleName]`

Example:
```csharp
namespace DatabridgeServer.Services.Products
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
```

### Model Development
- Use data annotations for validation (`[Required]`, `[StringLength]`, etc.)
- Include `CreatedAt` (DateTime, auto-set) and `UpdatedAt` (DateTime?, nullable) for tracking
- Use appropriate data types (decimal for prices, int for IDs)
- Namespace should be `DatabridgeServer.Models`

Example:
```csharp
public class Product
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

### DbContext Configuration
- Configure entity relationships and constraints in `OnModelCreating`
- Use Fluent API for complex configurations
- Set default values for timestamps using `HasDefaultValueSql("GETUTCDATE()")`

## Dependency Injection Registration

When creating a new service, always register it in `Program.cs`:
```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```

## Database Conventions
- Connection string is in `appsettings.json` under `ConnectionStrings:DefaultConnection`
- Database name: `DatabridgeDB`
- Use Entity Framework Core migrations for database changes
- Always use async methods for database operations
- Use `localhost\SQLEXPRESS` as the SQL Server instance

## API Design Standards
- RESTful API conventions
- Route pattern: `api/[controller]`
- Endpoints:
  - GET `/api/[resource]` - Get all
  - GET `/api/[resource]/{id}` - Get by ID
  - POST `/api/[resource]` - Create
  - PUT `/api/[resource]/{id}` - Update
  - DELETE `/api/[resource]/{id}` - Delete

## Error Handling
- Return appropriate HTTP status codes
- Include descriptive error messages
- Example: `return NotFound(new { message = $"Resource with ID {id} not found" });`
- Handle `DbUpdateConcurrencyException` in update operations

## CORS Configuration
- CORS policy named "AllowAll" is configured
- Allows any origin, method, and header (adjust for production)

## Swagger/OpenAPI
- Swagger UI is enabled in development mode
- Access at: `https://localhost:7162/swagger`

## When Adding New Modules

1. **Create Model** in `Models/[ModelName].cs`
2. **Add DbSet** to `ApplicationDbContext.cs`
3. **Create Service Interface** in `Services/[ModuleName]/I[ModuleName]Service.cs`
4. **Create Service Implementation** in `Services/[ModuleName]/[ModuleName]Service.cs`
5. **Create Controller** in `Controllers/[ModuleName]/[ModuleName]Controller.cs`
6. **Register Service** in `Program.cs` DI container
7. **Create Migration**: `dotnet ef migrations add Add[ModuleName]`
8. **Update Database**: `dotnet ef database update`

## Best Practices
- Use meaningful variable names
- Follow async/await pattern consistently
- Use UTC for all timestamps
- Validate model state in controllers before processing
- Keep controllers focused on HTTP concerns only
- Put all business logic in services
- Use constructor injection for dependencies
- Return consistent response formats

## Testing Strategy
- Controllers should be testable by mocking services
- Services should be testable by mocking DbContext
- Use in-memory database for integration tests

## Project Ports
- HTTPS: `https://localhost:7162`
- HTTP: `http://localhost:5071`

## Notes
- Framework: .NET 10.0
- ORM: Entity Framework Core 10.0.2
- Database: SQL Server (localhost\SQLEXPRESS)
- API Documentation: Swagger/OpenAPI
