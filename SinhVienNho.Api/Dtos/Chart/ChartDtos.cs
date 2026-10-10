namespace SinhVienNho.Api.Dtos.Chart;

public record ChartSummaryResponse(
    int Id,
    string Name,
    string Slug,
    string Image,
    string AvatarImage,
    string Price,
    string Difficulty,
    string Material,
    string Hook,
    string Time,
    string Description,
    int? CategoryId,
    string? CategoryName
);

public record ChartDetailResponse(
    int Id,
    string Name,
    string Slug,
    string Image,
    string AvatarImage,
    List<string>? FullChartImages,
    string Price,
    string Difficulty,
    string Material,
    string Hook,
    string Time,
    string Description,
    List<string> Includes,
    bool IsPurchased,
    int? CategoryId,
    string? CategoryName
);

public record ChartRequest(
    string Name,
    string Slug,
    string Image,
    string AvatarImage,
    List<string> FullChartImages,
    string Price,
    string Difficulty,
    string Material,
    string Hook,
    string Time,
    string Description,
    List<string> Includes,
    int? CategoryId
);
