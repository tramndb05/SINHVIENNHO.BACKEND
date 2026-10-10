namespace SinhVienNho.Api.Dtos.Cart;

public record CartItemResponse(int Id, int ProductId, string ProductSlug, string ProductName, string ProductImage, string ProductPrice, int Quantity);

public record AddCartItemRequest(int ProductId, int Quantity = 1);

public record UpdateCartItemRequest(int Quantity);
