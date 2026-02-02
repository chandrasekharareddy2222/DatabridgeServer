# DatabridgeServer - Product API

A .NET Web API project for managing products with full CRUD operations.

## Features

- **Product Management**: Complete CRUD operations (Create, Read, Update, Delete)
- **Entity Framework Core**: Database access with SQL Server
- **Swagger UI**: Interactive API documentation
- **CORS Enabled**: Cross-Origin Resource Sharing support
- **Data Validation**: Built-in model validation

## Database Structure

### Product Model
- `Id` (int) - Primary key
- `Name` (string, required, max 100 chars)
- `Description` (string, optional, max 500 chars)
- `Price` (decimal, required, > 0)
- `Stock` (int, >= 0)
- `CreatedAt` (DateTime, auto-set)
- `UpdatedAt` (DateTime, nullable)

## API Endpoints

### Products Controller (`/api/Products`)

- **GET** `/api/Products` - Get all products
- **GET** `/api/Products/{id}` - Get a specific product by ID
- **POST** `/api/Products` - Create a new product
- **PUT** `/api/Products/{id}` - Update an existing product
- **DELETE** `/api/Products/{id}` - Delete a product

## Setup Instructions

### 1. Database Setup

The connection string is configured in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=DatabridgeDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
}
```

### 2. Create Database Migration

Run the following commands in the project directory:

```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Update the database
dotnet ef database update
```

### 3. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7xxx`
- HTTP: `http://localhost:5xxx`
- Swagger UI: `https://localhost:7xxx/swagger`

## Sample API Requests

### Create a Product (POST)
```json
{
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "stock": 50
}
```

### Update a Product (PUT)
```json
{
  "id": 1,
  "name": "Gaming Laptop",
  "description": "High-performance gaming laptop",
  "price": 1299.99,
  "stock": 30
}
```

## Project Structure

```
DatabridgeServer/
├── Controllers/
│   └── ProductsController.cs    # Product API endpoints
├── Data/
│   └── ApplicationDbContext.cs  # EF Core DbContext
├── Models/
│   └── Product.cs               # Product entity model
├── Program.cs                   # Application configuration
└── appsettings.json            # Configuration settings
```

## Technologies Used

- **.NET 10.0** - Web API framework
- **Entity Framework Core 10.0.2** - ORM for database access
- **SQL Server** - Database
- **Swagger/OpenAPI** - API documentation
- **ASP.NET Core** - Web framework

## Future Enhancements

This project is designed to be modular. You can easily add more modules such as:
- Categories
- Orders
- Customers
- Inventory Management
- Authentication & Authorization

## Notes

- The database will be created automatically when you run the migration
- CORS is enabled for all origins (modify for production use)
- All timestamps use UTC
- Model validation is enforced automatically
