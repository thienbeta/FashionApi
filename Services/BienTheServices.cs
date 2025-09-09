using FashionApi.Data;
using FashionApi.DTO;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FashionApi.Services
{
    public class BienTheServices : IBienTheServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediaServices _mediaServices;
        private readonly IMemoryCacheServices _cacheServices;
        private readonly ILogger<BienTheServices> _logger;

        public BienTheServices(
            ApplicationDbContext context,
            IMediaServices mediaServices,
            IMemoryCacheServices cacheServices,
            ILogger<BienTheServices> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaServices = mediaServices ?? throw new ArgumentNullException(nameof(mediaServices));
            _cacheServices = cacheServices ?? throw new ArgumentNullException(nameof(cacheServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BienTheView> CreateAsync(BienTheCreate bienTheCreate, IFormFile imageFile)
        {
            _logger.LogInformation("Bắt đầu tạo biến thể mới: GiaBan={GiaBan}, SoLuongNhap={SoLuongNhap}",
                bienTheCreate.GiaBan, bienTheCreate.SoLuongNhap);

            if (bienTheCreate == null)
                throw new ArgumentException("Thông tin biến thể không hợp lệ.");

            // Validate MaSanPham
            if (!await _context.SanPhams.AnyAsync(sp => sp.MaSanPham == bienTheCreate.MaSanPham && sp.TrangThai == 1))
                throw new ArgumentException("Sản phẩm không tồn tại hoặc đã bị vô hiệu hóa.");

            // Validate MaMau
            if (!await IsValidDanhMuc(bienTheCreate.MaMau, 5))
                throw new ArgumentException("Màu sắc không hợp lệ.");

            // Validate MaKichThuoc
            if (!await IsValidDanhMuc(bienTheCreate.MaKichThuoc, 4))
                throw new ArgumentException("Kích thước không hợp lệ.");

            // ✅ Check trùng Màu + Kích thước cho sản phẩm
            if (await _context.BienThes.AnyAsync(bt =>
                bt.MaSanPham == bienTheCreate.MaSanPham &&
                bt.MaMau == bienTheCreate.MaMau &&
                bt.MaKichThuoc == bienTheCreate.MaKichThuoc))
            {
                throw new ArgumentException("Biến thể với màu và kích thước này đã tồn tại cho sản phẩm.");
            }

            try
            {
                string imageUrl = bienTheCreate.HinhAnh;
                if (imageFile != null)
                {
                    if (!imageFile.ContentType.StartsWith("image/"))
                        throw new ArgumentException("Tệp không phải hình ảnh.");
                    if (imageFile.Length > 5 * 1024 * 1024)
                        throw new ArgumentException("Kích thước tệp hình ảnh không được vượt quá 5MB.");

                    imageUrl = await _mediaServices.SaveOptimizedImageAsync(imageFile, "bienthe");
                }

                var bienThe = new BienThe
                {
                    HinhAnh = imageUrl,
                    GiaBan = bienTheCreate.GiaBan,
                    GiaNhap = bienTheCreate.GiaNhap,
                    SoLuongNhap = bienTheCreate.SoLuongNhap,
                    SoLuongBan = bienTheCreate.SoLuongBan,
                    KhuyenMai = bienTheCreate.KhuyenMai,
                    MaVach = bienTheCreate.MaVach,
                    MaSanPham = bienTheCreate.MaSanPham,
                    MaKichThuoc = bienTheCreate.MaKichThuoc,
                    MaMau = bienTheCreate.MaMau,
                    NgayTao = DateTime.UtcNow,
                    TrangThai = 1
                };

                _context.BienThes.Add(bienThe);
                await _context.SaveChangesAsync();

                // Xóa cache cũ
                _cacheServices.Remove("BienThe_All");
                _cacheServices.Remove($"BienThe_{bienThe.MaBienThe}");
                _cacheServices.Remove($"BienThe_SanPham_{bienThe.MaSanPham}");

                return await GetByIdAsync(bienThe.MaBienThe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo biến thể.");
                throw;
            }
        }
        public async Task<BienTheView> UpdateAsync(int id, BienTheEdit bienTheEdit, IFormFile imageFile = null)
        {
            _logger.LogInformation("Bắt đầu cập nhật biến thể: MaBienThe={Id}, GiaBan={GiaBan}", id, bienTheEdit?.GiaBan);

            if (bienTheEdit == null)
                throw new ArgumentException("Thông tin cập nhật không hợp lệ.");

            try
            {
                var bienThe = await _context.BienThes
                    .FirstOrDefaultAsync(bt => bt.MaBienThe == id);

                if (bienThe == null)
                    throw new KeyNotFoundException("Biến thể không tồn tại.");

                // Validate MaSanPham (nếu có thay đổi)
                int newMaSanPham = bienTheEdit.MaSanPham ?? bienThe.MaSanPham ?? 0;
                if (bienTheEdit.MaSanPham.HasValue &&
                    !await _context.SanPhams.AnyAsync(sp => sp.MaSanPham == newMaSanPham && sp.TrangThai == 1))
                    throw new ArgumentException("Sản phẩm không tồn tại hoặc đã bị vô hiệu hóa.");

                // Validate MaMau (nếu có thay đổi)
                int newMaMau = bienTheEdit.MaMau ?? bienThe.MaMau;
                if (bienTheEdit.MaMau.HasValue && !await IsValidDanhMuc(newMaMau, 5))
                    throw new ArgumentException("Màu sắc không hợp lệ.");

                // Validate MaKichThuoc (nếu có thay đổi)
                int newMaKichThuoc = bienTheEdit.MaKichThuoc ?? bienThe.MaKichThuoc;
                if (bienTheEdit.MaKichThuoc.HasValue && !await IsValidDanhMuc(newMaKichThuoc, 4))
                    throw new ArgumentException("Kích thước không hợp lệ.");

                // ✅ Check trùng Màu + Kích thước trong cùng sản phẩm
                bool isDuplicate = await _context.BienThes.AnyAsync(bt =>
                    bt.MaBienThe != id &&
                    bt.MaSanPham == newMaSanPham &&
                    bt.MaMau == newMaMau &&
                    bt.MaKichThuoc == newMaKichThuoc);

                if (isDuplicate)
                    throw new ArgumentException("Biến thể với màu và kích thước này đã tồn tại cho sản phẩm.");

                // ✅ Xử lý hình ảnh
                string imageUrl = bienThe.HinhAnh;
                if (imageFile != null)
                {
                    if (!imageFile.ContentType.StartsWith("image/"))
                        throw new ArgumentException("Tệp không phải hình ảnh.");

                    if (imageFile.Length > 5 * 1024 * 1024)
                        throw new ArgumentException("Kích thước tệp hình ảnh không được vượt quá 5MB.");

                    // Xóa hình ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(bienThe.HinhAnh))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", bienThe.HinhAnh.TrimStart('/'));
                        if (File.Exists(oldImagePath))
                        {
                            File.Delete(oldImagePath);
                            _logger.LogInformation("Đã xóa hình ảnh cũ: {OldImagePath}", oldImagePath);
                        }
                    }

                    imageUrl = await _mediaServices.SaveOptimizedImageAsync(imageFile, "bienthe");
                    _logger.LogInformation("Hình ảnh mới đã được lưu: {ImageUrl}", imageUrl);
                }

                // ✅ Cập nhật các thuộc tính
                bienThe.HinhAnh = imageUrl;
                bienThe.GiaBan = bienTheEdit.GiaBan ?? bienThe.GiaBan;
                bienThe.GiaNhap = bienTheEdit.GiaNhap ?? bienThe.GiaNhap;
                bienThe.SoLuongNhap = bienTheEdit.SoLuongNhap ?? bienThe.SoLuongNhap;
                bienThe.SoLuongBan = bienTheEdit.SoLuongBan ?? bienThe.SoLuongBan;
                bienThe.KhuyenMai = bienTheEdit.KhuyenMai ?? bienThe.KhuyenMai;
                bienThe.MaVach = bienTheEdit.MaVach ?? bienThe.MaVach;
                bienThe.TrangThai = bienTheEdit.TrangThai ?? bienThe.TrangThai;
                bienThe.MaSanPham = newMaSanPham;
                bienThe.MaMau = newMaMau;
                bienThe.MaKichThuoc = newMaKichThuoc;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Biến thể được cập nhật thành công: MaBienThe={Id}", id);

                // ✅ Clear cache
                _cacheServices.Remove($"BienThe_{id}");
                _cacheServices.Remove("BienThe_All");
                _cacheServices.Remove($"BienThe_SanPham_{bienThe.MaSanPham}");

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật biến thể: MaBienThe={Id}", id);
                throw;
            }
        }



        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Bắt đầu xóa biến thể: MaBienThe={Id}", id);

            try
            {
                var bienThe = await _context.BienThes.FirstOrDefaultAsync(bt => bt.MaBienThe == id);
                if (bienThe == null)
                {
                    _logger.LogWarning("Biến thể không tồn tại: MaBienThe={Id}", id);
                    return false;
                }

                if (!string.IsNullOrEmpty(bienThe.HinhAnh))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", bienThe.HinhAnh.TrimStart('/'));
                    if (File.Exists(imagePath))
                    {
                        File.Delete(imagePath);
                        _logger.LogInformation("Đã xóa hình ảnh: {ImagePath}", imagePath);
                    }
                }

                _context.BienThes.Remove(bienThe);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Biến thể được xóa thành công: MaBienThe={Id}", id);

                // Clear cache
                _cacheServices.Remove($"BienThe_{id}");
                _cacheServices.Remove("BienThe_All");
                if (bienThe.MaSanPham.HasValue)
                {
                    _cacheServices.Remove($"BienThe_SanPham_{bienThe.MaSanPham.Value}");
                }
                _logger.LogDebug("Đã xóa cache: BienThe_{Id}, BienThe_All, BienThe_SanPham_{MaSanPham}", id, bienThe.MaSanPham);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa biến thể: MaBienThe={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        public async Task<BienTheView> GetByIdAsync(int id)
        {
            _logger.LogInformation("Truy vấn biến thể: MaBienThe={Id}", id);

            try
            {
                return await _cacheServices.GetOrCreateAsync($"BienThe_{id}", async () =>
                {
                    var bienThe = await _context.BienThes
                        .AsNoTracking()
                        .Include(bt => bt.DanhMucMau)
                        .Include(bt => bt.DanhMucKichThuoc)
                        .FirstOrDefaultAsync(bt => bt.MaBienThe == id);

                    if (bienThe == null)
                    {
                        _logger.LogWarning("Không tìm thấy biến thể: MaBienThe={Id}", id);
                        return null;
                    }

                    return MapToView(bienThe);
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn biến thể: MaBienThe={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        public async Task<List<BienTheView>> GetAllAsync()
        {
            _logger.LogInformation("Truy vấn tất cả biến thể");

            try
            {
                return await _cacheServices.GetOrCreateAsync("BienThe_All", async () =>
                {
                    var bienThes = await _context.BienThes
                        .AsNoTracking()
                        .Include(bt => bt.DanhMucMau)
                        .Include(bt => bt.DanhMucKichThuoc)
                        .ToListAsync();

                    return bienThes.Select(MapToView).ToList();
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn tất cả biến thể, StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        public async Task<List<BienTheView>> SearchAsync(decimal? giaBan, int? soLuongNhap, int? trangThai)
        {
            _logger.LogInformation("Tìm kiếm biến thể: GiaBan={GiaBan}, SoLuongNhap={SoLuongNhap}, TrangThai={TrangThai}",
                giaBan, soLuongNhap, trangThai);

            try
            {
                var query = _context.BienThes
                    .AsNoTracking()
                    .Include(bt => bt.DanhMucMau)
                    .Include(bt => bt.DanhMucKichThuoc)
                    .AsQueryable();

                if (giaBan.HasValue)
                {
                    query = query.Where(bt => bt.GiaBan <= giaBan.Value);
                }

                if (soLuongNhap.HasValue)
                {
                    query = query.Where(bt => bt.SoLuongNhap >= soLuongNhap.Value);
                }

                if (trangThai.HasValue)
                {
                    query = query.Where(bt => bt.TrangThai == trangThai.Value);
                }

                var bienThes = await query.ToListAsync();
                return bienThes.Select(MapToView).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm biến thể, StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        public async Task<List<BienTheView>> FilterByDanhMucAsync(int maDanhMuc)
        {
            _logger.LogInformation("Lọc biến thể theo danh mục: MaDanhMuc={MaDanhMuc}", maDanhMuc);

            try
            {
                var danhMuc = await _context.DanhMucs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dm => dm.MaDanhMuc == maDanhMuc && dm.TrangThai == 1);

                if (danhMuc == null)
                {
                    _logger.LogWarning("Danh mục không tồn tại hoặc đã bị vô hiệu hóa: MaDanhMuc={MaDanhMuc}", maDanhMuc);
                    return new List<BienTheView>();
                }

                var query = _context.BienThes
                    .AsNoTracking()
                    .Include(bt => bt.DanhMucMau)
                    .Include(bt => bt.DanhMucKichThuoc)
                    .AsQueryable();

                if (danhMuc.LoaiDanhMuc == 4) // Kích thước
                {
                    query = query.Where(bt => bt.MaKichThuoc == maDanhMuc);
                }
                else if (danhMuc.LoaiDanhMuc == 5) // Màu sắc
                {
                    query = query.Where(bt => bt.MaMau == maDanhMuc);
                }
                else
                {
                    _logger.LogWarning("Loại danh mục không hợp lệ cho biến thể: MaDanhMuc={MaDanhMuc}, LoaiDanhMuc={LoaiDanhMuc}", maDanhMuc, danhMuc.LoaiDanhMuc);
                    return new List<BienTheView>();
                }

                var bienThes = await query.ToListAsync();
                return bienThes.Select(MapToView).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc biến thể theo danh mục: MaDanhMuc={MaDanhMuc}, StackTrace: {StackTrace}", maDanhMuc, ex.StackTrace);
                throw;
            }
        }

        public async Task<List<BienTheView>> GetBySanPhamAsync(int maSanPham)
        {
            _logger.LogInformation("Lấy biến thể theo sản phẩm: MaSanPham={MaSanPham}", maSanPham);

            try
            {
                return await _cacheServices.GetOrCreateAsync($"BienThe_SanPham_{maSanPham}", async () =>
                {
                    var bienThes = await _context.BienThes
                        .AsNoTracking()
                        .Include(bt => bt.DanhMucMau)
                        .Include(bt => bt.DanhMucKichThuoc)
                        .Where(bt => bt.MaSanPham == maSanPham)
                        .ToListAsync();

                    return bienThes.Select(MapToView).ToList();
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy biến thể theo sản phẩm: MaSanPham={MaSanPham}, StackTrace: {StackTrace}", maSanPham, ex.StackTrace);
                throw;
            }
        }

        private async Task<bool> IsValidDanhMuc(int maDanhMuc, int loaiDanhMuc)
        {
            return await _context.DanhMucs
                .AsNoTracking()
                .AnyAsync(dm => dm.MaDanhMuc == maDanhMuc && dm.LoaiDanhMuc == loaiDanhMuc && dm.TrangThai == 1);
        }

        private BienTheView MapToView(BienThe bienThe)
        {
            return new BienTheView
            {
                MaBienThe = bienThe.MaBienThe,
                HinhAnh = bienThe.HinhAnh,
                GiaBan = bienThe.GiaBan,
                GiaNhap = bienThe.GiaNhap,
                SoLuongNhap = bienThe.SoLuongNhap,
                SoLuongBan = bienThe.SoLuongBan,
                KhuyenMai = bienThe.KhuyenMai,
                MaVach = bienThe.MaVach,
                TrangThai = bienThe.TrangThai,
                NgayTao = bienThe.NgayTao,
                MaSanPham = bienThe.MaSanPham ?? 0, // Handle nullable if needed
                GiaTri = bienThe.GiaBan - (bienThe.GiaBan * (bienThe.KhuyenMai / 100)),
                MaKichThuoc = bienThe.MaKichThuoc,
                TenKichThuoc = bienThe.DanhMucKichThuoc?.TenDanhMuc,
                MaMau = bienThe.MaMau,
                TenMau = bienThe.DanhMucMau?.TenDanhMuc
            };
        }
    }
}