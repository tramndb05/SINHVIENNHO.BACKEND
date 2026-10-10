namespace SinhVienNho.Api.Dtos.User;

public record AdminUserResponse(int Id, string Name, string Phone, string Address, string? Email, string Role, DateTime CreatedAt);

public record UpdateUserRoleRequest(string Role);
