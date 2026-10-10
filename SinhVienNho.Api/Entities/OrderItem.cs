namespace SinhVienNho.Api.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public int? ChartId { get; set; }
    public Chart? Chart { get; set; }

    public int Quantity { get; set; }
    public string UnitPrice { get; set; } = string.Empty;
}
