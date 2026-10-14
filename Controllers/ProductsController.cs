using AdvancedEFCoreTask.Data;
using AdvancedEFCoreTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEFCoreTask.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/products?page=1&pageSize=10&search=phone
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Page must be positive and pageSize must be between 1 and 100.");
        }

        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.CategoryId,
                p.RowVersion,
                CategoryName = p.Category != null
                    ? p.Category.Name
                    : null
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            products
        });
    }

    // PUT: api/products/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] UpdateProductRequest request)
    {
        if (request.RowVersion is not { Length: > 0 })
        {
            return BadRequest("RowVersion is required.");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        // Update only the fields allowed by this endpoint.
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;

        // Use the version the client originally received.
        _context.Entry(product)
            .Property(p => p.RowVersion)
            .OriginalValue = request.RowVersion;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                "The product was changed by another user. Reload it and try again.");
        }
        catch (DbUpdateException)
        {
            return BadRequest(
                "The update failed. Check that the supplied category exists.");
        }

        return NoContent();
    }

    // DELETE: api/products/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> SoftDeleteProduct(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        product.IsDeleted = true;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}