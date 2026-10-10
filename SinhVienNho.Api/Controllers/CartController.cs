using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinhVienNho.Api.Data;
using SinhVienNho.Api.Dtos.Cart;
using SinhVienNho.Api.Entities;
using SinhVienNho.Api.Services;

namespace SinhVienNho.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;

    public CartController(AppDbContext db)
    {
        _db = db;
    }

    private static CartItemResponse ToResponse(CartItem ci) =>
        new(ci.Id, ci.ProductId, ci.Product!.Slug, ci.Product.Name, ci.Product.Image, ci.Product.Price, ci.Quantity);

    [HttpGet]
    public async Task<ActionResult<List<CartItemResponse>>> GetCart()
    {
        var userId = User.GetUserId();

        var items = await _db.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        return Ok(items.Select(ToResponse));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartItemResponse>> AddItem(AddCartItemRequest request)
    {
        var userId = User.GetUserId();

        var product = await _db.Products.FindAsync(request.ProductId);
        if (product is null) return NotFound(new { message = "Sản phẩm không tồn tại." });

        var quantity = request.Quantity <= 0 ? 1 : request.Quantity;

        var existing = await _db.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == request.ProductId);

        if (existing is not null)
        {
            existing.Quantity += quantity;
            await _db.SaveChangesAsync();
            return Ok(ToResponse(existing));
        }

        var item = new CartItem { UserId = userId, ProductId = request.ProductId, Quantity = quantity, Product = product };
        _db.CartItems.Add(item);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(item));
    }

    [HttpPut("items/{id:int}")]
    public async Task<ActionResult<CartItemResponse>> UpdateItem(int id, UpdateCartItemRequest request)
    {
        var userId = User.GetUserId();

        var item = await _db.CartItems.Include(ci => ci.Product).FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);
        if (item is null) return NotFound();

        if (request.Quantity <= 0)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        item.Quantity = request.Quantity;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(item));
    }

    [HttpDelete("items/{id:int}")]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var userId = User.GetUserId();

        var item = await _db.CartItems.FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);
        if (item is null) return NotFound();

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = User.GetUserId();

        var items = _db.CartItems.Where(ci => ci.UserId == userId);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
