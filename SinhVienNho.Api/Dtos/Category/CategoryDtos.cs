using SinhVienNho.Api.Entities.Enums;

namespace SinhVienNho.Api.Dtos.Category;

public record CategoryResponse(int Id, string Name, string Slug, string Type);

public record CategoryRequest(string Name, string Slug, CategoryType Type);
