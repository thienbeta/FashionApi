using System;
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
                    logger?.LogInformation("Người dùng đã tồn tại, bỏ qua seed user.");
                    return;
                }

                var admin = new NguoiDung
                {
                    HoTen = "Admin Demo",
                    NgaySinh = new DateTime(1990, 1, 1),
                    Sdt = "0123456789",
                    Email = "admin@example.com",
                    TaiKhoan = "admin",
                    MatKhau = PasswordHasher.HashPassword("admin"),
                    VaiTro = 1, // ví dụ 1 = admin
                    TrangThai = 1,
                    Avt = null,
                    TieuSu = "Tài khoản admin mẫu",
                    NgayTao = DateTime.UtcNow,
                    GioiTinh = 0
                };

                var user = new NguoiDung
                {
                    HoTen = "User Demo",
                    NgaySinh = new DateTime(1995, 6, 15),
                    Sdt = "0987654321",
                    Email = "user@example.com",
                    TaiKhoan = "user",
                    MatKhau = PasswordHasher.HashPassword("user"),
                    VaiTro = 0, // ví dụ 0 = regular user
                    TrangThai = 1,
                    Avt = null,
                    TieuSu = "Tài khoản user mẫu",
                    NgayTao = DateTime.UtcNow,
                    GioiTinh = 0
                };

                context.NguoiDungs.AddRange(admin, user);
                await context.SaveChangesAsync();

                logger?.LogInformation("Seed user thành công: admin và user đã được tạo.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Lỗi khi seed dữ liệu người dùng: {Message}", ex.Message);
                throw;
            }
        }
    }
}
