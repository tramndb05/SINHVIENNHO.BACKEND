namespace SinhVienNho.Api.Entities;

public class Chart
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string AvatarImage { get; set; } = string.Empty;
    public List<string> FullChartImages { get; set; } = new();
    public string Price { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string Hook { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Includes { get; set; } = new();

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<PurchasedChart> PurchasedCharts { get; set; } = new List<PurchasedChart>();
}
