using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinhVienNho.Api.Data;
using SinhVienNho.Api.Dtos.Order;
using SinhVienNho.Api.Entities;
using SinhVienNho.Api.Entities.Enums;
using SinhVienNho.Api.Services;

namespace SinhVienNho.Api.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    private static OrderResponse ToResponse(Order o) => new(
        o.Id, o.UserId, o.User?.Name ?? string.Empty, o.Status.ToString(), o.ShippingAddress, o.CreatedAt,
        o.Items.Select(i => new OrderItemResponse(
            i.Id, i.ProductId, i.Product?.Name, i.ChartId, i.Chart?.Name, i.Quantity, i.UnitPrice
        )).ToList()
    );

    [HttpPost("orders")]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var userId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
        {
            return BadRequest(new { message = "Vui lòng nhập địa chỉ giao hàng." });
        }

        var cartItems = await _db.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
        var chartIds = request.ChartIds?.Distinct().ToList() ?? new List<int>();
        var charts = chartIds.Count > 0
            ? await _db.Charts.Where(c => chartIds.Contains(c.Id)).ToListAsync()
            : new List<Chart>();

        if (cartItems.Count == 0 && charts.Count == 0)
        {
            return BadRequest(new { message = "Không có sản phẩm hoặc chart nào để đặt hàng." });
        }

        var order = new Order
        {
            UserId = userId,
            ShippingAddress = request.ShippingAddress,
            Status = OrderStatus.Pending
        };

        foreach (var ci in cartItems)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product!.Price
            });
        }

        foreach (var chart in charts)
        {
            order.Items.Add(new OrderItem
            {
                ChartId = chart.Id,
                Quantity = 1,
                UnitPrice = chart.Price
            });
        }

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cartItems);

        foreach (var chart in charts)
        {
            var alreadyPurchased = await _db.PurchasedCharts.AnyAsync(pc => pc.UserId == userId && pc.ChartId == chart.Id);
            if (!alreadyPurchased)
            {
                _db.PurchasedCharts.Add(new PurchasedChart { UserId = userId, ChartId = chart.Id });
            }
        }

        await _db.SaveChangesAsync();
        await _db.Entry(order).Reference(o => o.User).LoadAsync();
        await _db.Entry(order).Collection(o => o.Items).Query().Include(i => i.Product).Include(i => i.Chart).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToResponse(order));
    }

    [HttpGet("orders/me")]
    public async Task<ActionResult<List<OrderResponse>>> GetMyOrders()
    {
        var userId = User.GetUserId();

        var orders = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Chart)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(ToResponse));
    }

    [HttpGet("orders/{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Chart)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();
        if (!isAdmin && order.UserId != userId) return Forbid();

        return Ok(ToResponse(order));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/orders")]
    public async Task<ActionResult<List<OrderResponse>>> GetAll([FromQuery] OrderStatus? status)
    {
        var query = _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Chart)
            .AsQueryable();

        if (status is not null)
        {
            query = query.Where(o => o.Status == status);
        }

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(ToResponse));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("admin/orders/{id:int}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Chart)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
        {
            return BadRequest(new { message = "Trạng thái không hợp lệ." });
        }

        order.Status = newStatus;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(order));
    }
}
