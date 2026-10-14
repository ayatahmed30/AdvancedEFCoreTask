# Advanced EF Core Web API

An ASP.NET Core Web API demonstrating advanced Entity Framework Core features using SQL Server.

## Features

- **AsNoTracking:** Improves read-only query performance.
- **Projection:** Uses `Select` to return only the required product fields.
- **Pagination:** Supports `page` and `pageSize` query parameters.
- **Category Data:** Returns the category name with each product without the N+1 query problem.
- **Database Index:** Adds an index on `Product.Name` to support name-based searches.
- **Soft Delete:** Uses `IsDeleted` and a global query filter instead of physically deleting products.
- **Change Tracking:** Updates only the permitted product fields.
- **Concurrency Handling:** Uses SQL Server `rowversion` and handles `DbUpdateConcurrencyException`.

## Technologies

- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server

## Configuration

Update the `DefaultConnection` connection string in `appsettings.json` to match your local SQL Server instance.

Do not commit passwords or other sensitive credentials.

## Run the Project

```bash
dotnet restore
dotnet build
dotnet run
```

Use the local URL shown in the terminal to test the API.

## API Endpoints

### Get products

`GET /api/products?page=1&pageSize=10`

Optional search:

`GET /api/products?page=1&pageSize=10&search=phone`

### Update a product

`PUT /api/products/{id}`

The request must include the current `rowVersion` returned by GET.

### Soft delete a product

`DELETE /api/products/{id}`

## Concurrency Testing

A successful update returns `204 No Content`. An update using a stale `rowVersion` returns `409 Conflict`.

## Database Index

An index on `Product.Name` supports name-based filtering. Its actual performance benefit depends on the query and database execution plan.
