using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SinhVienNho.Api.Data;
using SinhVienNho.Api.Dtos.Auth;
using SinhVienNho.Api.Entities;
using SinhVienNho.Api.Services;

namespace SinhVienNho.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwtService;

    public AuthController(AppDbContext db, JwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Name, user.Phone, user.Address, user.Email, user.Role.ToString());

    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Phone) ||
            string.IsNullOrWhiteSpace(request.Address) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Vui lòng nhập đầy đủ họ tên, số điện thoại, địa chỉ và mật khẩu." });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự." });
        }

        if (await _db.Users.AnyAsync(u => u.Phone == request.Phone))
        {
            return Conflict(new { message = "Số điện thoại này đã được đăng ký." });
        }

        var user = new User
        {
            Name = request.Name,
            Phone = request.Phone,
            Address = request.Address,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);
        return Ok(new AuthResponse(token, ToResponse(user)));
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Số điện thoại hoặc mật khẩu không đúng." });
        }

        var token = _jwtService.GenerateToken(user);
        return Ok(new AuthResponse(token, ToResponse(user)));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user is null) return NotFound();

        return Ok(ToResponse(user));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserResponse>> UpdateMe(UpdateMeRequest request)
    {
        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest(new { message = "Vui lòng nhập đầy đủ họ tên và địa chỉ." });
        }

        user.Name = request.Name;
        user.Address = request.Address;
        user.Email = request.Email;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(user));
    }
}
