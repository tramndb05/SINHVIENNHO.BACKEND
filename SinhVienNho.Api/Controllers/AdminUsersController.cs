using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinhVienNho.Api.Data;
using SinhVienNho.Api.Dtos.User;
using SinhVienNho.Api.Entities;
using SinhVienNho.Api.Entities.Enums;

namespace SinhVienNho.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminUsersController(AppDbContext db)
    {
        _db = db;
    }

    private static AdminUserResponse ToResponse(User u) =>
        new(u.Id, u.Name, u.Phone, u.Address, u.Email, u.Role.ToString(), u.CreatedAt);

    [HttpGet]
    public async Task<ActionResult<List<AdminUserResponse>>> GetAll()
    {
        var users = await _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return Ok(users.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminUserResponse>> GetById(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        return Ok(ToResponse(user));
    }

    [HttpPut("{id:int}/role")]
    public async Task<ActionResult<AdminUserResponse>> UpdateRole(int id, UpdateUserRoleRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (!Enum.TryParse<Role>(request.Role, true, out var newRole))
        {
            return BadRequest(new { message = "Vai trò không hợp lệ." });
        }

        user.Role = newRole;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(user));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
