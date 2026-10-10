namespace SinhVienNho.Api.Dtos.Order;

public record OrderItemResponse(int Id, int? ProductId, string? ProductName, int? ChartId, string? ChartName, int Quantity, string UnitPrice);

public record OrderResponse(int Id, int UserId, string UserName, string Status, string ShippingAddress, DateTime CreatedAt, List<OrderItemResponse> Items);

public record CreateOrderRequest(string ShippingAddress, List<int> ChartIds);

public record UpdateOrderStatusRequest(string Status);
