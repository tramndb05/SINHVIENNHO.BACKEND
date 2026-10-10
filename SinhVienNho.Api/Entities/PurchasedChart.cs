namespace SinhVienNho.Api.Entities;

public class PurchasedChart
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int ChartId { get; set; }
    public Chart? Chart { get; set; }

    public int? OrderId { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
}
