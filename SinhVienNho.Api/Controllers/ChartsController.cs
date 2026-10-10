using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinhVienNho.Api.Data;
using SinhVienNho.Api.Dtos.Chart;
using SinhVienNho.Api.Entities;
using SinhVienNho.Api.Services;

namespace SinhVienNho.Api.Controllers;

[ApiController]
[Route("api/charts")]
public class ChartsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ChartsController(AppDbContext db)
    {
        _db = db;
    }

    private static ChartSummaryResponse ToSummary(Chart c) =>
        new(c.Id, c.Name, c.Slug, c.Image, c.AvatarImage, c.Price, c.Difficulty, c.Material, c.Hook, c.Time, c.Description, c.CategoryId, c.Category?.Name);

    [HttpGet]
    public async Task<ActionResult<List<ChartSummaryResponse>>> GetAll([FromQuery] int? categoryId)
    {
        var query = _db.Charts.Include(c => c.Category).AsQueryable();

        if (categoryId is not null)
        {
            query = query.Where(c => c.CategoryId == categoryId);
        }

        var charts = await query.OrderByDescending(c => c.Id).ToListAsync();
        return Ok(charts.Select(ToSummary));
    }

    [Authorize]
    [HttpGet("purchased")]
    public async Task<ActionResult<List<ChartSummaryResponse>>> GetPurchased()
    {
        var userId = User.GetUserId();

        var charts = await _db.PurchasedCharts
            .Where(pc => pc.UserId == userId)
            .Include(pc => pc.Chart).ThenInclude(c => c!.Category)
            .OrderByDescending(pc => pc.Id)
            .Select(pc => pc.Chart!)
            .ToListAsync();

        return Ok(charts.Select(ToSummary));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ChartDetailResponse>> GetBySlug(string slug)
    {
        var chart = await _db.Charts.Include(c => c.Category).FirstOrDefaultAsync(c => c.Slug == slug);
        if (chart is null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        var isPurchased = isAdmin;

        if (!isAdmin && User.Identity?.IsAuthenticated == true)
        {
            var userId = User.GetUserId();
            isPurchased = await _db.PurchasedCharts.AnyAsync(pc => pc.UserId == userId && pc.ChartId == chart.Id);
        }

        return Ok(new ChartDetailResponse(
            chart.Id, chart.Name, chart.Slug, chart.Image, chart.AvatarImage,
            isPurchased ? chart.FullChartImages : null,
            chart.Price, chart.Difficulty, chart.Material, chart.Hook, chart.Time,
            chart.Description, chart.Includes, isPurchased, chart.CategoryId, chart.Category?.Name
        ));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ChartSummaryResponse>> Create(ChartRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug) ||
            string.IsNullOrWhiteSpace(request.Image) || string.IsNullOrWhiteSpace(request.Price))
        {
            return BadRequest(new { message = "Vui lòng nhập tên chart, slug, ảnh và giá." });
        }

        if (await _db.Charts.AnyAsync(c => c.Slug == request.Slug))
        {
            return Conflict(new { message = "Slug chart đã tồn tại." });
        }

        var chart = new Chart
        {
            Name = request.Name,
            Slug = request.Slug,
            Image = request.Image,
            AvatarImage = string.IsNullOrWhiteSpace(request.AvatarImage) ? request.Image : request.AvatarImage,
            FullChartImages = request.FullChartImages ?? new List<string>(),
            Price = request.Price,
            Difficulty = request.Difficulty,
            Material = request.Material,
            Hook = request.Hook,
            Time = request.Time,
            Description = request.Description,
            Includes = request.Includes ?? new List<string>(),
            CategoryId = request.CategoryId
        };

        _db.Charts.Add(chart);
        await _db.SaveChangesAsync();
        await _db.Entry(chart).Reference(c => c.Category).LoadAsync();

        return CreatedAtAction(nameof(GetBySlug), new { slug = chart.Slug }, ToSummary(chart));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ChartSummaryResponse>> Update(int id, ChartRequest request)
    {
        var chart = await _db.Charts.Include(c => c.Category).FirstOrDefaultAsync(c => c.Id == id);
        if (chart is null) return NotFound();

        if (await _db.Charts.AnyAsync(c => c.Slug == request.Slug && c.Id != id))
        {
            return Conflict(new { message = "Slug chart đã tồn tại." });
        }

        chart.Name = request.Name;
        chart.Slug = request.Slug;
        chart.Image = request.Image;
        chart.AvatarImage = string.IsNullOrWhiteSpace(request.AvatarImage) ? request.Image : request.AvatarImage;
        chart.FullChartImages = request.FullChartImages ?? new List<string>();
        chart.Price = request.Price;
        chart.Difficulty = request.Difficulty;
        chart.Material = request.Material;
        chart.Hook = request.Hook;
        chart.Time = request.Time;
        chart.Description = request.Description;
        chart.Includes = request.Includes ?? new List<string>();
        chart.CategoryId = request.CategoryId;

        await _db.SaveChangesAsync();
        await _db.Entry(chart).Reference(c => c.Category).LoadAsync();

        return Ok(ToSummary(chart));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var chart = await _db.Charts.FindAsync(id);
        if (chart is null) return NotFound();

        _db.Charts.Remove(chart);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
