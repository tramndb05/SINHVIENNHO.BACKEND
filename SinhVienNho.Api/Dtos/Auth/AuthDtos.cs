namespace SinhVienNho.Api.Dtos.Auth;

public record RegisterRequest(string Name, string Phone, string Address, string? Email, string Password);

public record LoginRequest(string Phone, string Password);

public record UpdateMeRequest(string Name, string Address, string? Email);

public record AuthResponse(string Token, UserResponse User);

public record UserResponse(int Id, string Name, string Phone, string Address, string? Email, string Role);
