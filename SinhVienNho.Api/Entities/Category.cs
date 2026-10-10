using SinhVienNho.Api.Entities.Enums;

namespace SinhVienNho.Api.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public CategoryType Type { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Chart> Charts { get; set; } = new List<Chart>();
}
