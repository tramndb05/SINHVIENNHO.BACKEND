using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinhVienNho.Api.Data;
using SinhVienNho.Api.Dtos.Product;
using SinhVienNho.Api.Entities;

namespace SinhVienNho.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.Name, p.Slug, p.Image, p.Price, p.Description, p.Details, p.CategoryId, p.Category?.Name);

    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAll([FromQuery] int? categoryId)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (categoryId is not null)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        var products = await query.OrderByDescending(p => p.Id).ToListAsync();
        return Ok(products.Select(ToResponse));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ProductResponse>> GetBySlug(string slug)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Slug == slug);
        if (product is null) return NotFound();

        return Ok(ToResponse(product));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(ProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug) ||
            string.IsNullOrWhiteSpace(request.Image) || string.IsNullOrWhiteSpace(request.Price))
        {
            return BadRequest(new { message = "Vui lòng nhập tên sản phẩm, slug, ảnh và giá." });
        }

        if (await _db.Products.AnyAsync(p => p.Slug == request.Slug))
        {
            return Conflict(new { message = "Slug sản phẩm đã tồn tại." });
        }

        var product = new Product
        {
            Name = request.Name,
            Slug = request.Slug,
            Image = request.Image,
            Price = request.Price,
            Description = request.Description,
            Details = request.Details ?? new List<string>(),
            CategoryId = request.CategoryId
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();

        return CreatedAtAction(nameof(GetBySlug), new { slug = product.Slug }, ToResponse(product));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, ProductRequest request)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        if (await _db.Products.AnyAsync(p => p.Slug == request.Slug && p.Id != id))
        {
            return Conflict(new { message = "Slug sản phẩm đã tồn tại." });
        }

        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Image = request.Image;
        product.Price = request.Price;
        product.Description = request.Description;
        product.Details = request.Details ?? new List<string>();
        product.CategoryId = request.CategoryId;

        await _db.SaveChangesAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();

        return Ok(ToResponse(product));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
