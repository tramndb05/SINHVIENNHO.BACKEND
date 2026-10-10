namespace SinhVienNho.Api.Dtos.Product;

public record ProductResponse(
    int Id,
    string Name,
    string Slug,
    string Image,
    string Price,
    string Description,
    List<string> Details,
    int? CategoryId,
    string? CategoryName
);

public record ProductRequest(
    string Name,
    string Slug,
    string Image,
    string Price,
    string Description,
    List<string> Details,
    int? CategoryId
);
