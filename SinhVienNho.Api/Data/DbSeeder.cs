using Microsoft.EntityFrameworkCore;
using SinhVienNho.Api.Entities;
using SinhVienNho.Api.Entities.Enums;

namespace SinhVienNho.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        db.Database.Migrate();

        if (!db.Users.Any(u => u.Role == Role.Admin))
        {
            db.Users.Add(new User
            {
                Name = "Quản trị viên",
                Phone = "0900000000",
                Address = "SinhVienNho.handmade",
                Email = "admin@sinhviennho.handmade",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = Role.Admin
            });
        }

        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category { Name = "Phụ kiện len", Slug = "phu-kien-len", Type = CategoryType.Product },
                new Category { Name = "Trang phục len", Slug = "trang-phuc-len", Type = CategoryType.Product },
                new Category { Name = "Chart thú bông", Slug = "chart-thu-bong", Type = CategoryType.Chart },
                new Category { Name = "Chart đồ trang trí", Slug = "chart-do-trang-tri", Type = CategoryType.Chart }
            );
        }

        db.SaveChanges();

        var trangPhuc = db.Categories.First(c => c.Slug == "trang-phuc-len");
        var phuKien = db.Categories.First(c => c.Slug == "phu-kien-len");
        var chartThuBong = db.Categories.First(c => c.Slug == "chart-thu-bong");
        var chartTrangTri = db.Categories.First(c => c.Slug == "chart-do-trang-tri");

        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product
                {
                    Name = "Chân váy len nhung đũa xoè 2 tầng",
                    Slug = "chanvay2tang",
                    Image = "/images/chanvaylen1.1.jpg",
                    Price = "Liên hệ",
                    Description = "Chân váy len handmade được móc từ len nhung đũa mềm mại, thiết kế xoè 2 tầng nữ tính, phù hợp để phối với áo len hoặc áo croptop.",
                    Details = new List<string> { "Sản phẩm handmade", "Chất liệu len mềm", "Kiểu dáng xoè 2 tầng", "Phù hợp đi chơi, chụp ảnh và mặc hằng ngày" },
                    CategoryId = trangPhuc.Id
                },
                new Product
                {
                    Name = "Set áo dây chân váy A len cotton",
                    Slug = "setaovay",
                    Image = "/images/setaovay.png",
                    Price = "Liên hệ",
                    Description = "Set áo dây phối chân váy chữ A bằng len cotton, mang phong cách trẻ trung, dễ thương và phù hợp với thời trang handmade.",
                    Details = new List<string> { "Gồm áo dây và chân váy", "Chất liệu len cotton", "Thiết kế trẻ trung", "Phù hợp phối đồ nữ tính" },
                    CategoryId = trangPhuc.Id
                },
                new Product
                {
                    Name = "Khăn len bướm cotton",
                    Slug = "khanbuom",
                    Image = "/images/khanchoangbuom.png",
                    Price = "Liên hệ",
                    Description = "Khăn len họa tiết bướm được làm thủ công, thích hợp dùng làm khăn choàng hoặc phụ kiện thời trang.",
                    Details = new List<string> { "Sản phẩm handmade", "Họa tiết bướm nổi bật", "Chất liệu len cotton", "Dễ phối với nhiều kiểu trang phục" },
                    CategoryId = phuKien.Id
                },
                new Product
                {
                    Name = "Túi hoa hướng dương len nhung đũa",
                    Slug = "tuihoahh",
                    Image = "/images/tuihoahuongduong.png",
                    Price = "Liên hệ",
                    Description = "Túi len handmade phối hoa hướng dương nổi bật, phù hợp làm phụ kiện đi chơi hoặc làm quà tặng.",
                    Details = new List<string> { "Túi len handmade", "Trang trí hoa hướng dương", "Kiểu dáng dễ thương", "Phù hợp làm quà tặng" },
                    CategoryId = phuKien.Id
                },
                new Product
                {
                    Name = "Dây áo thời trang xanh tím",
                    Slug = "dayaoxanhtim",
                    Image = "/images/dayaotim2.jpg",
                    Price = "Liên hệ",
                    Description = "Dây áo thời trang màu xanh tím được móc bằng len, dùng để phối trang phục hoặc làm phụ kiện handmade.",
                    Details = new List<string> { "Phụ kiện handmade", "Màu xanh tím nổi bật", "Dễ phối đồ", "Thiết kế đơn giản, dễ sử dụng" },
                    CategoryId = phuKien.Id
                },
                new Product
                {
                    Name = "Dây áo thời trang đỏ",
                    Slug = "dayaodo",
                    Image = "/images/dayaodo1.jpg",
                    Price = "Liên hệ",
                    Description = "Dây áo thời trang màu đỏ được làm thủ công, phù hợp dùng làm phụ kiện phối đồ.",
                    Details = new List<string> { "Phụ kiện handmade", "Màu đỏ nổi bật", "Dễ phối trang phục", "Thiết kế nhỏ gọn" },
                    CategoryId = phuKien.Id
                }
            );
        }

        if (!db.Charts.Any())
        {
            db.Charts.AddRange(
                new Chart
                {
                    Name = "Chart thỏ đeo túi",
                    Slug = "thodt",
                    Image = "/images/Chartthodtui.png",
                    AvatarImage = "/images/Chartthodtui.png",
                    FullChartImages = new List<string> { "/images/Chartthodtuishow.png" },
                    Price = "29.000đ",
                    Difficulty = "Trung bình",
                    Material = "Len milk cotton hoặc cotton baby",
                    Hook = "2.0mm - 2.5mm",
                    Time = "2 - 4 giờ",
                    Description = "Chart hướng dẫn móc thỏ mini đeo túi xinh xắn, phù hợp với người đã có kiến thức móc len cơ bản.",
                    Includes = new List<string> { "Hình ảnh mẫu chart", "Thông tin len và kim móc", "Ghi chú khi thực hiện sản phẩm" },
                    CategoryId = chartThuBong.Id
                },
                new Chart
                {
                    Name = "Chart cá voi",
                    Slug = "cavoi",
                    Image = "/images/Chartcavoi.png",
                    AvatarImage = "/images/Chartcavoi.png",
                    FullChartImages = new List<string> { "/images/chartcavoishow.png" },
                    Price = "25.000đ",
                    Difficulty = "Dễ",
                    Material = "Len milk cotton hoặc cotton baby",
                    Hook = "2.0mm - 2.5mm",
                    Time = "1 - 2 giờ",
                    Description = "Chart hướng dẫn móc cá voi mini dễ thương, phù hợp với người mới học móc len.",
                    Includes = new List<string> { "Hình ảnh mẫu chart", "Thông tin len và kim móc", "Hướng dẫn thực hiện cơ bản" },
                    CategoryId = chartThuBong.Id
                },
                new Chart
                {
                    Name = "Chart kitty mang mũ cáo",
                    Slug = "ktmc",
                    Image = "/images/Chartkittymcao.png",
                    AvatarImage = "/images/Chartkittymcao.png",
                    FullChartImages = new List<string> { "/images/Chartkittymcaoshow.png" },
                    Price = "29.000đ",
                    Difficulty = "Trung bình",
                    Material = "Len milk cotton hoặc cotton baby",
                    Hook = "2.0mm - 2.5mm",
                    Time = "2 - 3 giờ",
                    Description = "Chart hướng dẫn móc kitty mini đội mũ cáo đáng yêu, phù hợp làm móc khóa hoặc quà tặng handmade.",
                    Includes = new List<string> { "Hình ảnh mẫu chart", "Thông tin len và kim móc", "Ghi chú kỹ thuật khi móc" },
                    CategoryId = chartThuBong.Id
                },
                new Chart
                {
                    Name = "Chart capybara",
                    Slug = "capybara",
                    Image = "/images/Chartcpbr.png",
                    AvatarImage = "/images/Chartcpbr.png",
                    FullChartImages = new List<string> { "/images/Chartcpbrshow.png" },
                    Price = "29.000đ",
                    Difficulty = "Trung bình",
                    Material = "Len milk cotton, cotton baby hoặc len nhung nhỏ",
                    Hook = "2.0mm - 3.0mm",
                    Time = "3 - 5 giờ",
                    Description = "Chart hướng dẫn móc capybara đáng yêu, thích hợp cho người muốn làm thú bông len handmade.",
                    Includes = new List<string> { "Hình ảnh mẫu chart", "Thông tin len và kim móc", "Ghi chú kỹ thuật khi móc" },
                    CategoryId = chartThuBong.Id
                },
                new Chart
                {
                    Name = "Chart lót ly capybara",
                    Slug = "llcpbr",
                    Image = "/images/Chartllcpbr.png",
                    AvatarImage = "/images/Chartllcpbr.png",
                    FullChartImages = new List<string> { "/images/Chartllcpbrshow.png" },
                    Price = "19.000đ",
                    Difficulty = "Dễ",
                    Material = "Len cotton hoặc milk cotton",
                    Hook = "2.5mm - 3.0mm",
                    Time = "1 - 2 giờ",
                    Description = "Chart hướng dẫn móc lót ly hình capybara dễ thương, phù hợp cho người mới tập móc và muốn làm sản phẩm trang trí nhỏ.",
                    Includes = new List<string> { "Hình ảnh mẫu chart", "Thông tin len và kim móc", "Hướng dẫn thực hiện cơ bản" },
                    CategoryId = chartTrangTri.Id
                }
            );
        }

        db.SaveChanges();
    }
}
