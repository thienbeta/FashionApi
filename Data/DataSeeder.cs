using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FashionApi.DTO;
using FashionApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FashionApi.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("DataSeeder");

            try
            {
                // Nếu đã có user thì bỏ qua
                if (await context.NguoiDungs.AnyAsync())
                {
                    logger?.LogInformation("Người dùng đã tồn tại, bỏ qua seed dữ liệu.");
                    return;
                }

                // Seed NguoiDung
                var users = new List<NguoiDung>
                {
                    new NguoiDung
                    {
                        HoTen = "Admin Demo",
                        NgaySinh = new DateTime(1990, 1, 1),
                        Sdt = "0123456789",
                        Email = "admin@example.com",
                        TaiKhoan = "admin",
                        MatKhau = PasswordHasher.HashPassword("admin123@A"),
                        VaiTro = 1, // admin
                        TrangThai = 1,
                        Avt = null,
                        TieuSu = "Tài khoản admin mẫu",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 0
                    },
                    new NguoiDung
                    {
                        HoTen = "User Demo",
                        NgaySinh = new DateTime(1995, 6, 15),
                        Sdt = "0987654321",
                        Email = "user@example.com",
                        TaiKhoan = "user",
                        MatKhau = PasswordHasher.HashPassword("user123@A"),
                        VaiTro = 0, // regular user
                        TrangThai = 1,
                        Avt = null,
                        TieuSu = "Tài khoản user mẫu",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 0
                    },
                    new NguoiDung
                    {
                        HoTen = "Nguyễn Văn A",
                        NgaySinh = new DateTime(1988, 3, 20),
                        Sdt = "0912345678",
                        Email = "nguyenvana@example.com",
                        TaiKhoan = "nguyenvana",
                        MatKhau = PasswordHasher.HashPassword("password123"),
                        VaiTro = 0,
                        TrangThai = 1,
                        Avt = "/uploads/nguoidung/0b37586a-3b74-411c-ae16-c92c6f2cb5d9.webp",
                        TieuSu = "Người dùng mẫu A",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 1
                    },
                    new NguoiDung
                    {
                        HoTen = "Trần Thị B",
                        NgaySinh = new DateTime(1992, 7, 10),
                        Sdt = "0987654322",
                        Email = "tranthib@example.com",
                        TaiKhoan = "tranthib",
                        MatKhau = PasswordHasher.HashPassword("password123"),
                        VaiTro = 0,
                        TrangThai = 1,
                        Avt = "/uploads/nguoidung/03d7e014-a12d-451b-ac00-c5cc54512f53.webp",
                        TieuSu = "Người dùng mẫu B",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 2
                    },
                    new NguoiDung
                    {
                        HoTen = "Lê Văn C",
                        NgaySinh = new DateTime(1985, 12, 5),
                        Sdt = "0976543210",
                        Email = "levanc@example.com",
                        TaiKhoan = "levanc",
                        MatKhau = PasswordHasher.HashPassword("password123"),
                        VaiTro = 0,
                        TrangThai = 1,
                        Avt = "/uploads/nguoidung/5f143163-6d20-4bff-9db2-ba08de89a6a2.webp",
                        TieuSu = "Người dùng mẫu C",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 1
                    },
                    new NguoiDung
                    {
                        HoTen = "Phạm Thị D",
                        NgaySinh = new DateTime(1998, 4, 15),
                        Sdt = "0965432109",
                        Email = "phamthid@example.com",
                        TaiKhoan = "phamthid",
                        MatKhau = PasswordHasher.HashPassword("password123"),
                        VaiTro = 0,
                        TrangThai = 1,
                        Avt = "/uploads/nguoidung/719c9e1f-4fe0-452d-a1a9-88b509c815aa.webp",
                        TieuSu = "Người dùng mẫu D",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 2
                    },
                    new NguoiDung
                    {
                        HoTen = "Hoàng Văn E",
                        NgaySinh = new DateTime(1990, 9, 25),
                        Sdt = "0954321098",
                        Email = "hoangvane@example.com",
                        TaiKhoan = "hoangvane",
                        MatKhau = PasswordHasher.HashPassword("password123"),
                        VaiTro = 0,
                        TrangThai = 1,
                        Avt = "/uploads/nguoidung/a26e29e3-d5e0-462c-9a6a-f39f937c4090.webp",
                        TieuSu = "Người dùng mẫu E",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 1
                    },
                    new NguoiDung
                    {
                        HoTen = "Đỗ Thị F",
                        NgaySinh = new DateTime(1993, 11, 30),
                        Sdt = "0943210987",
                        Email = "dothif@example.com",
                        TaiKhoan = "dothif",
                        MatKhau = PasswordHasher.HashPassword("password123"),
                        VaiTro = 0,
                        TrangThai = 1,
                        Avt = "/uploads/nguoidung/d88a5258-b1a6-4105-a3b1-92c58d7cb065.webp",
                        TieuSu = "Người dùng mẫu F",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 2
                    },
                    new NguoiDung
                    {
                        HoTen = "Bùi Văn G",
                        NgaySinh = new DateTime(1987, 5, 8),
                        Sdt = "0932109876",
                        Email = "buivang@example.com",
                        TaiKhoan = "buivang",
                        MatKhau = PasswordHasher.HashPassword("password123"),
                        VaiTro = 0,
                        TrangThai = 1,
                        Avt = "/uploads/nguoidung/e87709fa-6791-4c5c-aaab-753100163eda.webp",
                        TieuSu = "Người dùng mẫu G",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 1
                    },
                    new NguoiDung
                    {
                        HoTen = "Vũ Thị H",
                        NgaySinh = new DateTime(1996, 2, 14),
                        Sdt = "0921098765",
                        Email = "vuthih@example.com",
                        TaiKhoan = "vuthih",
                        MatKhau = PasswordHasher.HashPassword("password123"),
                        VaiTro = 0,
                        TrangThai = 1,
                        Avt = "/uploads/nguoidung/f222a8f8-b1df-44e4-bead-9fd16644d021.webp",
                        TieuSu = "Người dùng mẫu H",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 2
                    },
                    new NguoiDung
                    {
                        HoTen = "Ngô Văn I",
                        NgaySinh = new DateTime(1989, 8, 22),
                        Sdt = "0910987654",
                        Email = "ngovani@example.com",
                        TaiKhoan = "ngovani",
                        MatKhau = PasswordHasher.HashPassword("password123"),
                        VaiTro = 0,
                        TrangThai = 1,
                        Avt = null,
                        TieuSu = "Người dùng mẫu I",
                        NgayTao = DateTime.UtcNow,
                        GioiTinh = 1
                    }
                };

                context.NguoiDungs.AddRange(users);
                await context.SaveChangesAsync();

                // Seed DanhMuc
                var categories = new List<DanhMuc>
                {
                    // Loại sản phẩm (1)
                    new DanhMuc { TenDanhMuc = "Áo", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/0eedd56d-17e7-462f-b9bf-30eb033eafc5.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Quần", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/3a82faf2-ebec-4d44-aea8-caf67a46b12d.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Váy", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/4aed3efd-5377-4edc-849c-289a743a0299.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Đồ lót", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/5d59fc2f-1818-4bf7-9d97-293e947e7c37.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Phụ kiện", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/6f2d181a-082d-4d95-9a5b-6362ad8ebe04.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Giày", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/7d5d3971-7768-415a-8013-867cdbae2072.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Túi xách", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/35c161f3-ea97-4c47-9db0-f7916ae7b9ed.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Nón", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/83f8a8a6-f743-43ef-8359-5ca7cc16df77.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Trang sức", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/85cadf9c-81a0-4795-aa92-a84f4d5bcc4b.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Đồ thể thao", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/311d09ac-8a7a-4a35-8e43-193d9ffcbb45.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Đồ mùa đông", LoaiDanhMuc = 1, HinhAnh = "/uploads/danhmuc/335bdb5e-0cf7-4d8d-9531-94acdc74af1a.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },

                    // Thương hiệu (2)
                    new DanhMuc { TenDanhMuc = "Nike", LoaiDanhMuc = 2, HinhAnh = "/uploads/danhmuc/904a36e0-7714-4f16-88b8-dce2bad9b2fc.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Adidas", LoaiDanhMuc = 2, HinhAnh = "/uploads/danhmuc/73148b11-ea01-478a-a2ce-44fa26565b7b.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Gucci", LoaiDanhMuc = 2, HinhAnh = "/uploads/danhmuc/a1de293d-14d9-49b4-81a1-03773020219e.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Louis Vuitton", LoaiDanhMuc = 2, HinhAnh = "/uploads/danhmuc/be668113-bc49-4de4-ac76-69554d8251c9.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Zara", LoaiDanhMuc = 2, HinhAnh = "/uploads/danhmuc/c6b9f2be-b984-45bc-8bf1-0946c0571cda.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "H&M", LoaiDanhMuc = 2, HinhAnh = "/uploads/danhmuc/cf551f46-44df-42fb-bb42-62b6acf776ba.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Uniqlo", LoaiDanhMuc = 2, HinhAnh = "/uploads/danhmuc/ed23fca2-b9b8-409e-8a28-77e3dc059d72.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "Chanel", LoaiDanhMuc = 2, HinhAnh = "/uploads/danhmuc/f77e4077-e02d-412c-a9ce-881b65e82de9.webp", NgayTao = DateTime.UtcNow, TrangThai = 1 },

                    // Hashtag (3)
                    new DanhMuc { TenDanhMuc = "#HoaiThuVn2024", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "#SummerStyle", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "#Streetwear", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "#Minimalist", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "#Vintage", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "#Luxury", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "#Casual", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "#Elegant", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "#Sporty", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "#Trendy", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new DanhMuc { TenDanhMuc = "#Classic", LoaiDanhMuc = 3, HinhAnh = null, NgayTao = DateTime.UtcNow, TrangThai = 1 }
                };

                context.DanhMucs.AddRange(categories);
                await context.SaveChangesAsync();

                // Get category IDs
                var loaiIds = categories.Where(c => c.LoaiDanhMuc == 1).Select(c => c.MaDanhMuc).ToList();
                var thuongHieuIds = categories.Where(c => c.LoaiDanhMuc == 2).Select(c => c.MaDanhMuc).ToList();
                var hashtagIds = categories.Where(c => c.LoaiDanhMuc == 3).Select(c => c.MaDanhMuc).ToList();

                // Seed SanPham
                var products = new List<SanPham>();
                var productNames = new[] { "Áo thun cơ bản", "Quần jean slim", "Váy maxi", "Đồ lót cotton", "Túi xách da", "Giày sneaker", "Nón bucket", "Vòng tay bạc", "Áo khoác len", "Quần short thể thao", "Đầm dạ hội" };
                var descriptions = new[] { "Áo thun chất liệu cotton thoáng mát", "Quần jean ôm body phong cách trẻ trung", "Váy dài thanh lịch cho các dịp đặc biệt", "Đồ lót cotton mềm mại, an toàn cho da", "Túi xách da thật cao cấp", "Giày sneaker êm ái cho mọi hoạt động", "Nón bucket thời trang", "Vòng tay bạc 925 tinh xảo", "Áo khoác len ấm áp mùa đông", "Quần short thể thao thấm hút mồ hôi", "Đầm dạ hội lấp lánh" };
                var barcodes = new[] { "8936000000001", "8936000000002", "8936000000003", "8936000000004", "8936000000005", "8936000000006", "8936000000007", "8936000000008", "8936000000009", "8936000000010", "8936000000011" };
                var prices = new[] { 150000m, 450000m, 350000m, 80000m, 1200000m, 800000m, 120000m, 250000m, 550000m, 200000m, 800000m };
                var salePrices = new[] { 120000m, 400000m, 300000m, 60000m, 1000000m, 700000m, 100000m, 200000m, 500000m, 180000m, 700000m };
                var quantities = new[] { 50, 30, 20, 100, 15, 25, 40, 10, 35, 60, 12 };
                var genders = new[] { 0, 1, 2, 2, 0, 0, 0, 0, 0, 1, 2 };

                for (int i = 0; i < 11; i++)
                {
                    products.Add(new SanPham
                    {
                        TenSanPham = productNames[i],
                        MoTa = descriptions[i],
                        Slug = productNames[i].ToLower().Replace(" ", "-"),
                        MaVach = barcodes[i],
                        NgayTao = DateTime.UtcNow,
                        TrangThai = 1,
                        GioiTinh = genders[i],
                        MaLoai = loaiIds[i % loaiIds.Count],
                        MaThuongHieu = thuongHieuIds[i % thuongHieuIds.Count],
                        MaHashtag = hashtagIds[i % hashtagIds.Count],
                        GiaBan = prices[i],
                        GiaSale = salePrices[i],
                        SoLuong = quantities[i]
                    });
                }

                context.SanPhams.AddRange(products);
                await context.SaveChangesAsync();

                // Get product IDs
                var productIds = products.Select(p => p.MaSanPham).ToList();

                // Seed BinhLuan
                var comments = new List<BinhLuan>();
                var commentTitles = new[] { "Sản phẩm tốt", "Chất lượng ổn", "Giao hàng nhanh", "Thiết kế đẹp", "Giá hợp lý", "Màu sắc đẹp", "Size vừa vặn", "Chất liệu tốt", "Phù hợp giá tiền", "Sẽ mua lại", "Rất hài lòng" };
                var commentContents = new[] { "Sản phẩm đúng như mô tả", "Chất lượng khá tốt", "Shop giao hàng rất nhanh", "Thiết kế rất đẹp và tinh tế", "Giá cả phải chăng", "Màu sắc rất tươi sáng", "Size vừa vặn với người Việt", "Chất liệu mềm mại", "Đáng đồng tiền bát gạo", "Sẽ ủng hộ shop lần sau", "Rất hài lòng với sản phẩm" };
                var ratings = new[] { 5, 4, 5, 5, 4, 5, 4, 5, 4, 5, 5 };

                for (int i = 0; i < 11; i++)
                {
                    comments.Add(new BinhLuan
                    {
                        TieuDe = commentTitles[i],
                        NoiDung = commentContents[i],
                        DanhGia = ratings[i],
                        NgayTao = DateTime.UtcNow,
                        TrangThai = 1,
                        MaNguoiDung = users[i % users.Count].MaNguoiDung,
                        MaSanPham = productIds[i % productIds.Count]
                    });
                }

                context.BinhLuans.AddRange(comments);
                await context.SaveChangesAsync();

                // Get comment IDs
                var commentIds = comments.Select(c => c.MaBinhLuan).ToList();

                // Seed GiaoDien
                var giaoDiens = new List<GiaoDien>
                {
                    new GiaoDien { TenGiaoDien = "Logo chính", LoaiGiaoDien = 1, MoTa = "Logo thương hiệu chính", MetaTitle = "HoaiThu.Vn Logo", MetaDescription = "Logo của thương hiệu thời trang HoaiThu.Vn", MetaKeywords = "logo, hoaithu.vn", NgayTao = DateTime.UtcNow, TrangThai = 1 },
                    new GiaoDien { TenGiaoDien = "Logo phụ", LoaiGiaoDien = 1, MoTa = "Logo nhỏ/phụ", MetaTitle = "Secondary Logo", MetaDescription = "Logo phụ của thương hiệu", MetaKeywords = "logo, secondary", NgayTao = DateTime.UtcNow, TrangThai = 0 }
                };

                context.GiaoDiens.AddRange(giaoDiens);
                await context.SaveChangesAsync();

                // Get giaoDien IDs
                var giaoDienIds = giaoDiens.Select(g => g.MaGiaoDien).ToList();

                // Seed Media
                var medias = new List<Media>();
                var mediaUrls = new[]
                {
                    "/uploads/sanpham/0a6c23e6-bf16-4d9a-bff3-8e45a4b45a46.webp",
                    "/uploads/sanpham/0d23b714-4ebf-440a-a2e7-f574aaafc9f2.webp",
                    "/uploads/sanpham/0dee5b62-fbc1-4d33-8af0-2fe33e5cf400.webp",
                    "/uploads/sanpham/2e92c77e-3261-4ccc-8cbd-ed8d1229810a.webp",
                    "/uploads/sanpham/2f085bbb-8abd-4126-b12f-bc79f7447aa9.webp",
                    "/uploads/sanpham/2fb938b7-e948-4f04-af22-6bd3bdf5a930.webp",
                    "/uploads/sanpham/3af939cd-105d-4b68-adc5-041be2a25dd3.webp",
                    "/uploads/sanpham/3c998b5a-8525-43a8-b5e0-5cbc1d3b3dd7.webp",
                    "/uploads/sanpham/3cb23378-0344-48fd-90b5-7aad6fec2f2e.webp",
                    "/uploads/sanpham/3f48ab83-1007-4a17-88b4-9a153887b13a.webp",
                    "/uploads/sanpham/4af41d14-f4c4-4877-bdc4-333cf9caa577.webp"
                };

                // Media for products
                for (int i = 0; i < 11; i++)
                {
                    medias.Add(new Media
                    {
                        LoaiMedia = "image",
                        DuongDan = mediaUrls[i],
                        AltMedia = $"Hình ảnh sản phẩm {i + 1}",
                        LinkMedia = null,
                        NgayTao = DateTime.UtcNow,
                        TrangThai = 1,
                        MaSanPham = productIds[i],
                        MaBinhLuan = null,
                        MaGiaoDien = null
                    });
                }

                // Media for comments
                var commentMediaUrls = new[]
                {
                    "/uploads/binhluan/1d6e10e3-f037-459a-a17a-87211d697d14.webp",
                    "/uploads/binhluan/2c4f6338-f71f-4f86-8fc0-abe0543a2a96.webp",
                    "/uploads/binhluan/3a957b2c-36d4-4da7-9c89-15252830d444.webp",
                    "/uploads/binhluan/4c5c9d58-b16f-4df8-b1ad-ec6920f562ea.webp",
                    "/uploads/binhluan/4fcec91d-85e4-4838-95e5-cb2f2d7451c0.webp",
                    "/uploads/binhluan/5bb82737-cc45-4cf9-99a2-865dd58d78a1.webp",
                    "/uploads/binhluan/5c1fbb3f-b22a-4f45-b762-87e60449b08c.webp",
                    "/uploads/binhluan/5ed4ba87-ca20-42b3-a880-5c6442997254.webp",
                    "/uploads/binhluan/6c6ca86d-06ce-49db-b5a5-992fcc68fd8e.webp",
                    "/uploads/binhluan/7a783b2b-3c81-47e1-9288-1201b0e22c8b.webp",
                    "/uploads/binhluan/7ae46735-cff5-406a-8ddd-fece03e5dc35.webp"
                };

                for (int i = 0; i < 11; i++)
                {
                    medias.Add(new Media
                    {
                        LoaiMedia = "image",
                        DuongDan = commentMediaUrls[i],
                        AltMedia = $"Hình ảnh bình luận {i + 1}",
                        LinkMedia = null,
                        NgayTao = DateTime.UtcNow,
                        TrangThai = 1,
                        MaSanPham = null,
                        MaBinhLuan = commentIds[i],
                        MaGiaoDien = null
                    });
                }

                // Media for giaoDien (logos, banners, etc.)
                var giaoDienMediaUrls = new[]
                {
                    "/uploads/sanpham/4c666fd2-00f7-42fa-9823-f4deb5ec02cd.webp", // using product images as placeholders
                    "/uploads/sanpham/4cd872ee-98bd-4b3d-9d63-f857f7f7c767.webp",
                    "/uploads/sanpham/4eb193a6-0b28-478c-81d5-8a5e91fe5bdf.webp",
                    "/uploads/sanpham/4eedee16-7306-42b4-b039-2788c2c5e4e5.webp",
                    "/uploads/sanpham/08afc649-961d-4c98-9031-4c71e0f218a7.webp",
                    "/uploads/sanpham/15b5ed9f-bdaa-408c-8064-2d73e670445e.webp",
                    "/uploads/sanpham/18c50723-5a69-4500-a4ee-69b6d07cf5c0.webp",
                    "/uploads/sanpham/22d7e474-012c-4c57-834b-c7cb6b185620.webp",
                    "/uploads/sanpham/22e07449-6a69-4d86-b0b6-049231c74d47.webp",
                    "/uploads/sanpham/23e5b097-fff7-4bd7-8dd3-8de641d549ff.webp",
                    "/uploads/sanpham/32d04d56-9910-42b6-bad2-a0974d49e4e0.webp"
                };

                for (int i = 0; i < giaoDienIds.Count; i++)
                {
                    string mediaUrl;
                    int trangThai;

                    if (i == 0) // Logo chính active
                    {
                        mediaUrl = "/uploads/logo/origin.png";
                        trangThai = 1;
                    }
                    else // Logo phụ (inactive)
                    {
                        mediaUrl = "/uploads/logo/origin-v1.png";
                        trangThai = 0;
                    }

                    medias.Add(new Media
                    {
                        LoaiMedia = "image",
                        DuongDan = mediaUrl,
                        AltMedia = i == 0 ? "Logo chính HoaiThu.Vn" : "Logo phụ HoaiThu.Vn",
                        LinkMedia = null,
                        NgayTao = DateTime.UtcNow,
                        TrangThai = trangThai,
                        MaSanPham = null,
                        MaBinhLuan = null,
                        MaGiaoDien = giaoDienIds[i]
                    });
                }

                context.Medias.AddRange(medias);
                await context.SaveChangesAsync();

                logger?.LogInformation("Seed dữ liệu thành công: tất cả bảng đã được tạo với dữ liệu mẫu.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Lỗi khi seed dữ liệu: {Message}", ex.Message);
                throw;
            }
        }
    }
}
