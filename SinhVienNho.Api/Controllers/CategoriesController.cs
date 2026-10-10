using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinhVienNho.Api.Data;
using SinhVienNho.Api.Dtos.Category;
using SinhVienNho.Api.Entities.Enums;

namespace SinhVienNho.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    private static CategoryResponse ToResponse(Entities.Category c) =>
        new(c.Id, c.Name, c.Slug, c.Type.ToString());

    [HttpGet]
    public async Task<ActionResult<List<CategoryResponse>>> GetAll([FromQuery] CategoryType? type)
    {
        var query = _db.Categories.AsQueryable();

        if (type is not null)
        {
            query = query.Where(c => c.Type == type);
        }

        var categories = await query.OrderBy(c => c.Name).ToListAsync();
        return Ok(categories.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        return Ok(ToResponse(category));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug))
        {
            return BadRequest(new { message = "Vui lòng nhập tên và slug danh mục." });
        }

        if (await _db.Categories.AnyAsync(c => c.Slug == request.Slug && c.Type == request.Type))
        {
            return Conflict(new { message = "Slug danh mục đã tồn tại." });
        }

        var category = new Entities.Category
        {
            Name = request.Name,
            Slug = request.Slug,
            Type = request.Type
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, ToResponse(category));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, CategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug))
        {
            return BadRequest(new { message = "Vui lòng nhập tên và slug danh mục." });
        }

        if (await _db.Categories.AnyAsync(c => c.Slug == request.Slug && c.Type == request.Type && c.Id != id))
        {
            return Conflict(new { message = "Slug danh mục đã tồn tại." });
        }

        category.Name = request.Name;
        category.Slug = request.Slug;
        category.Type = request.Type;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(category));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
