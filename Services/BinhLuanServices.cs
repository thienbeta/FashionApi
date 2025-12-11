using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FashionApi.Data;
using FashionApi.DTO;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FashionApi.Services
{
    public class BinhLuanServices : IBinhLuanServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediaServices _mediaServices;
        private readonly IMemoryCacheServices _cacheServices;
        private readonly ILogger<BinhLuanServices> _logger;

        public BinhLuanServices(
            ApplicationDbContext context,
            IMediaServices mediaServices,
            IMemoryCacheServices cacheServices,
            ILogger<BinhLuanServices> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaServices = mediaServices ?? throw new ArgumentNullException(nameof(mediaServices));
            _cacheServices = cacheServices ?? throw new ArgumentNullException(nameof(cacheServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BinhLuanView> CreateAsync(BinhLuanCreate model)
        {
            _logger.LogInformation("Bắt đầu tạo bình luận mới: MaNguoiDung={MaNguoiDung}, MaSanPham={MaSanPham}", model.MaNguoiDung, model.MaSanPham);

            if (model == null)
                throw new ArgumentException("Thông tin bình luận không hợp lệ.");

            // Validate user
            if (!await _context.NguoiDungs.AnyAsync(u => u.MaNguoiDung == model.MaNguoiDung))
                throw new ArgumentException("Người dùng không hợp lệ.");

            // Validate product (nếu có)
            SanPham? sanPham = null;
            if (model.MaSanPham.HasValue)
            {
                sanPham = await _context.SanPhams
                    .FirstOrDefaultAsync(sp => sp.MaSanPham == model.MaSanPham.Value && sp.TrangThai == 1);

                if (sanPham == null)
                    throw new ArgumentException("Sản phẩm không hợp lệ.");
            }

            // Validate số lượng hình ảnh (tối đa 5)
            if (model.Images != null && model.Images.Count > 5)
                throw new ArgumentException("Không được tải lên quá 5 hình ảnh.");

            try
            {
                var binhLuan = new BinhLuan
                {
                    TieuDe = model.TieuDe,
                    NoiDung = model.NoiDung,
                    DanhGia = model.DanhGia,
                    TrangThai = model.TrangThai,
                    MaNguoiDung = model.MaNguoiDung,
                    MaSanPham = model.MaSanPham,
                    NgayTao = DateTime.UtcNow
                };

                _context.BinhLuans.Add(binhLuan);
                await _context.SaveChangesAsync();

                // Lưu hình ảnh
                if (model.Images != null && model.Images.Any())
                {
                    foreach (var imageFile in model.Images)
                    {
                        if (!imageFile.ContentType.StartsWith("image/"))
                            throw new ArgumentException($"Tệp {imageFile.FileName} không phải hình ảnh.");

                        if (imageFile.Length > 5 * 1024 * 1024)
                            throw new ArgumentException($"Kích thước tệp {imageFile.FileName} không được vượt quá 5MB.");

                        // Lưu ảnh tối ưu
                        var imageUrl = await _mediaServices.SaveOptimizedImageAsync(imageFile, "binhluan");

                        // Tạo AltMedia
                        string noiDungMoTa = !string.IsNullOrEmpty(binhLuan.TieuDe)
                            ? binhLuan.TieuDe
                            : (!string.IsNullOrEmpty(binhLuan.NoiDung) ? binhLuan.NoiDung : Path.GetFileNameWithoutExtension(imageFile.FileName));

                        string altMedia = $"Sản phẩm {sanPham?.TenSanPham ?? "không xác định"} - ảnh trong bình luận {noiDungMoTa}";

                        // Giới hạn 70 ký tự
                        if (altMedia.Length > 70)
                            altMedia = altMedia.Substring(0, 67) + "...";

                        var media = new Media
                        {
                            MaBinhLuan = binhLuan.MaBinhLuan,
                            LoaiMedia = "image",
                            AltMedia = altMedia,
                            DuongDan = imageUrl,
                            TrangThai = 1,
                            NgayTao = DateTime.UtcNow
                        };

                        _context.Medias.Add(media);
                    }

                    await _context.SaveChangesAsync();
                }

                // Clear cache
                _cacheServices.Remove("BinhLuan_All");
                _cacheServices.Remove($"BinhLuan_{binhLuan.MaBinhLuan}");
                if (model.MaSanPham.HasValue)
                    _cacheServices.Remove($"BinhLuan_SanPham_{model.MaSanPham.Value}");

                return await GetByIdAsync(binhLuan.MaBinhLuan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bình luận: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<BinhLuanView> UpdateAsync(int id, BinhLuanEdit model, List<IFormFile>? newImageFiles = null)
        {
            _logger.LogInformation("Bắt đầu cập nhật bình luận: MaBinhLuan={Id}", id);

            if (model == null || id != model.MaBinhLuan)
                throw new ArgumentException("Thông tin cập nhật không hợp lệ.");

            try
            {
                var binhLuan = await _context.BinhLuans
                    .Include(bl => bl.Medias)
                    .FirstOrDefaultAsync(bl => bl.MaBinhLuan == id);

                if (binhLuan == null)
                    throw new KeyNotFoundException("Bình luận không tồn tại.");

                // Validate images (max 5, including existing)
                int currentImageCount = binhLuan.Medias?.Count(m => m.TrangThai == 1) ?? 0;
                if (newImageFiles != null && (currentImageCount + newImageFiles.Count > 5))
                    throw new ArgumentException("Tổng số hình ảnh không được vượt quá 5.");

                // Update properties
                if (model.TieuDe != null)
                    binhLuan.TieuDe = model.TieuDe;
                if (model.NoiDung != null)
                    binhLuan.NoiDung = model.NoiDung;
                if (model.DanhGia.HasValue)
                    binhLuan.DanhGia = model.DanhGia.Value;
                if (model.TrangThai.HasValue)
                    binhLuan.TrangThai = model.TrangThai.Value;

                // Add new images
                if (newImageFiles != null && newImageFiles.Any())
                {
                    foreach (var imageFile in newImageFiles)
                    {
                        if (!imageFile.ContentType.StartsWith("image/"))
                            throw new ArgumentException($"Tệp {imageFile.FileName} không phải hình ảnh.");

                        if (imageFile.Length > 5 * 1024 * 1024)
                            throw new ArgumentException($"Kích thước tệp {imageFile.FileName} không được vượt quá 5MB.");

                        var imageUrl = await _mediaServices.SaveOptimizedImageAsync(imageFile, "binhluan");

                        var media = new Media
                        {
                            MaBinhLuan = binhLuan.MaBinhLuan,
                            LoaiMedia = "image",
                            DuongDan = imageUrl,
                            TrangThai = 1
                        };
                        _context.Medias.Add(media);
                    }

                    _logger.LogInformation("Đã thêm {Count} hình ảnh mới cho bình luận: MaBinhLuan={Id}", newImageFiles.Count, id);
                }

                await _context.SaveChangesAsync();

                // Clear cache
                _cacheServices.Remove($"BinhLuan_{id}");
                _cacheServices.Remove("BinhLuan_All");
                if (binhLuan.MaSanPham.HasValue)
                    _cacheServices.Remove($"BinhLuan_SanPham_{binhLuan.MaSanPham.Value}");

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bình luận: MaBinhLuan={Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateStatusAsync(int id, int trangThai)
        {
            _logger.LogInformation("Cập nhật trạng thái bình luận: MaBinhLuan={Id}, TrangThai={TrangThai}", id, trangThai);

            try
            {
                var binhLuan = await _context.BinhLuans.FindAsync(id);

                if (binhLuan == null)
                {
                    _logger.LogWarning("Bình luận không tồn tại: MaBinhLuan={Id}", id);
                    return false;
                }

                binhLuan.TrangThai = trangThai;
                await _context.SaveChangesAsync();

                // Clear cache
                _cacheServices.Remove($"BinhLuan_{id}");
                _cacheServices.Remove("BinhLuan_All");
                if (binhLuan.MaSanPham.HasValue)
                    _cacheServices.Remove($"BinhLuan_SanPham_{binhLuan.MaSanPham.Value}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái bình luận: MaBinhLuan={Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Bắt đầu xóa bình luận: MaBinhLuan={Id}", id);

            try
            {
                var binhLuan = await _context.BinhLuans
                    .Include(bl => bl.Medias)
                    .FirstOrDefaultAsync(bl => bl.MaBinhLuan == id);

                if (binhLuan == null)
                {
                    _logger.LogWarning("Bình luận không tồn tại: MaBinhLuan={Id}", id);
                    return false;
                }

                // Xóa các file hình ảnh liên kết trước khi xóa mềm
                if (binhLuan.Medias != null && binhLuan.Medias.Any())
                {
                    foreach (var media in binhLuan.Medias)
                    {
                        if (!string.IsNullOrEmpty(media.DuongDan))
                        {
                            var fileDeleted = await _mediaServices.DeleteImageAsync(media.DuongDan);
                            if (fileDeleted)
                            {
                                _logger.LogInformation("Đã xóa file hình ảnh: {ImagePath}", media.DuongDan);
                            }
                            else
                            {
                                _logger.LogWarning("Không thể xóa file hình ảnh: {ImagePath}", media.DuongDan);
                            }
                        }
                    }
                }

                // Soft delete
                binhLuan.TrangThai = 0;
                foreach (var media in binhLuan.Medias)
                {
                    media.TrangThai = 0;
                }

                await _context.SaveChangesAsync();

                // Clear cache
                _cacheServices.Remove($"BinhLuan_{id}");
                _cacheServices.Remove("BinhLuan_All");
                if (binhLuan.MaSanPham.HasValue)
                    _cacheServices.Remove($"BinhLuan_SanPham_{binhLuan.MaSanPham.Value}");

                _logger.LogInformation("Xóa bình luận thành công: MaBinhLuan={Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bình luận: MaBinhLuan={Id}", id);
                throw;
            }
        }

        public async Task<BinhLuanView> GetByIdAsync(int id)
        {
            _logger.LogInformation("Truy vấn bình luận: MaBinhLuan={Id}", id);

            try
            {
                return await _cacheServices.GetOrCreateAsync($"BinhLuan_{id}", async () =>
                {
                    var binhLuan = await _context.BinhLuans
                        .AsNoTracking()
                        .Include(bl => bl.Medias)
                        .Include(bl => bl.NguoiDungNavigation)
                        .Include(bl => bl.SanPhamNavigation)
                        .FirstOrDefaultAsync(bl => bl.MaBinhLuan == id);

                    if (binhLuan == null)
                    {
                        _logger.LogWarning("Không tìm thấy bình luận: MaBinhLuan={Id}", id);
                        return null;
                    }

                    // Calculate rating statistics for the product
                    var ratingStats = await _context.BinhLuans
                        .Where(bl => bl.MaSanPham == binhLuan.MaSanPham && bl.TrangThai == 1 && bl.DanhGia.HasValue)
                        .GroupBy(bl => bl.MaSanPham)
                        .Select(g => new
                        {
                            DanhGiaTrungBinh = (decimal?)g.Average(bl => bl.DanhGia.Value),
                            SoLuongDanhGia = g.Count(),
                            SoLuong5Sao = g.Count(bl => bl.DanhGia == 5),
                            SoLuong4Sao = g.Count(bl => bl.DanhGia == 4),
                            SoLuong3Sao = g.Count(bl => bl.DanhGia == 3),
                            SoLuong2Sao = g.Count(bl => bl.DanhGia == 2),
                            SoLuong1Sao = g.Count(bl => bl.DanhGia == 1)
                        })
                        .FirstOrDefaultAsync();

                    return MapToView(binhLuan, ratingStats?.DanhGiaTrungBinh, ratingStats?.SoLuongDanhGia,
                        ratingStats?.SoLuong5Sao, ratingStats?.SoLuong4Sao, ratingStats?.SoLuong3Sao,
                        ratingStats?.SoLuong2Sao, ratingStats?.SoLuong1Sao);
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn bình luận: MaBinhLuan={Id}", id);
                throw;
            }
        }

        public async Task<List<BinhLuanView>> GetAllAsync()
        {
            _logger.LogInformation("Truy vấn tất cả bình luận");

            try
            {
                return await _cacheServices.GetOrCreateAsync("BinhLuan_All", async () =>
                {
                    var binhLuans = await _context.BinhLuans
                        .AsNoTracking()
                        .Include(bl => bl.Medias)
                        .Include(bl => bl.NguoiDungNavigation)
                        .Include(bl => bl.SanPhamNavigation)
                        .OrderByDescending(bl => bl.NgayTao)
                        .ToListAsync();

                    // Calculate rating statistics for each product
                    var ratingStats = await _context.BinhLuans
                        .Where(bl => bl.TrangThai == 1 && bl.DanhGia.HasValue)
                        .GroupBy(bl => bl.MaSanPham)
                        .Select(g => new
                        {
                            MaSanPham = g.Key,
                            DanhGiaTrungBinh = (decimal?)g.Average(bl => bl.DanhGia.Value),
                            SoLuongDanhGia = g.Count(),
                            SoLuong5Sao = g.Count(bl => bl.DanhGia == 5),
                            SoLuong4Sao = g.Count(bl => bl.DanhGia == 4),
                            SoLuong3Sao = g.Count(bl => bl.DanhGia == 3),
                            SoLuong2Sao = g.Count(bl => bl.DanhGia == 2),
                            SoLuong1Sao = g.Count(bl => bl.DanhGia == 1)
                        })
                        .ToDictionaryAsync(x => x.MaSanPham, x => (
                            x.DanhGiaTrungBinh,
                            x.SoLuongDanhGia,
                            x.SoLuong5Sao,
                            x.SoLuong4Sao,
                            x.SoLuong3Sao,
                            x.SoLuong2Sao,
                            x.SoLuong1Sao));

                    return binhLuans.Select(bl =>
                        MapToView(bl,
                            ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].DanhGiaTrungBinh : null,
                            ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuongDanhGia : 0,
                            ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong5Sao : 0,
                            ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong4Sao : 0,
                            ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong3Sao : 0,
                            ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong2Sao : 0,
                            ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong1Sao : 0))
                        .ToList();
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn tất cả bình luận");
                throw;
            }
        }

        public async Task<List<BinhLuanView>> SearchAsync(int? danhGia, int? trangThai, int? maSanPham, int? maNguoiDung)
        {
            _logger.LogInformation("Tìm kiếm bình luận: DanhGia={DanhGia}, TrangThai={TrangThai}, MaSanPham={MaSanPham}, MaNguoiDung={MaNguoiDung}",
                danhGia, trangThai, maSanPham, maNguoiDung);

            try
            {
                var query = _context.BinhLuans
                    .AsNoTracking()
                    .Include(bl => bl.Medias)
                    .Include(bl => bl.NguoiDungNavigation)
                    .Include(bl => bl.SanPhamNavigation)
                    .AsQueryable();

                if (danhGia.HasValue)
                    query = query.Where(bl => bl.DanhGia == danhGia.Value);

                if (trangThai.HasValue)
                    query = query.Where(bl => bl.TrangThai == trangThai.Value);

                if (maSanPham.HasValue)
                    query = query.Where(bl => bl.MaSanPham == maSanPham.Value);

                if (maNguoiDung.HasValue)
                    query = query.Where(bl => bl.MaNguoiDung == maNguoiDung.Value);

                query = query.OrderByDescending(bl => bl.NgayTao);

                var binhLuans = await query.ToListAsync();

                // Calculate rating statistics for each product
                var ratingStats = await _context.BinhLuans
                    .Where(bl => bl.TrangThai == 1 && bl.DanhGia.HasValue)
                    .GroupBy(bl => bl.MaSanPham)
                    .Select(g => new
                    {
                        MaSanPham = g.Key,
                        DanhGiaTrungBinh = (decimal?)g.Average(bl => bl.DanhGia.Value),
                        SoLuongDanhGia = g.Count(),
                        SoLuong5Sao = g.Count(bl => bl.DanhGia == 5),
                        SoLuong4Sao = g.Count(bl => bl.DanhGia == 4),
                        SoLuong3Sao = g.Count(bl => bl.DanhGia == 3),
                        SoLuong2Sao = g.Count(bl => bl.DanhGia == 2),
                        SoLuong1Sao = g.Count(bl => bl.DanhGia == 1)
                    })
                    .ToDictionaryAsync(x => x.MaSanPham, x => (
                        x.DanhGiaTrungBinh,
                        x.SoLuongDanhGia,
                        x.SoLuong5Sao,
                        x.SoLuong4Sao,
                        x.SoLuong3Sao,
                        x.SoLuong2Sao,
                        x.SoLuong1Sao));

                return binhLuans.Select(bl =>
                    MapToView(bl,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].DanhGiaTrungBinh : null,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuongDanhGia : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong5Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong4Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong3Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong2Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong1Sao : 0))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm bình luận");
                throw;
            }
        }

        public async Task<List<BinhLuanView>> FilterByTrangThaiAsync(int trangThai)
        {
            _logger.LogInformation("Lọc bình luận theo trạng thái: TrangThai={TrangThai}", trangThai);

            try
            {
                var binhLuans = await _context.BinhLuans
                    .AsNoTracking()
                    .Include(bl => bl.Medias)
                    .Include(bl => bl.NguoiDungNavigation)
                    .Include(bl => bl.SanPhamNavigation)
                    .Where(bl => bl.TrangThai == trangThai)
                    .ToListAsync();

                // Calculate rating statistics for each product
                var ratingStats = await _context.BinhLuans
                    .Where(bl => bl.TrangThai == 1 && bl.DanhGia.HasValue)
                    .GroupBy(bl => bl.MaSanPham)
                    .Select(g => new
                    {
                        MaSanPham = g.Key,
                        DanhGiaTrungBinh = (decimal?)g.Average(bl => bl.DanhGia.Value),
                        SoLuongDanhGia = g.Count(),
                        SoLuong5Sao = g.Count(bl => bl.DanhGia == 5),
                        SoLuong4Sao = g.Count(bl => bl.DanhGia == 4),
                        SoLuong3Sao = g.Count(bl => bl.DanhGia == 3),
                        SoLuong2Sao = g.Count(bl => bl.DanhGia == 2),
                        SoLuong1Sao = g.Count(bl => bl.DanhGia == 1)
                    })
                    .ToDictionaryAsync(x => x.MaSanPham, x => (
                        x.DanhGiaTrungBinh,
                        x.SoLuongDanhGia,
                        x.SoLuong5Sao,
                        x.SoLuong4Sao,
                        x.SoLuong3Sao,
                        x.SoLuong2Sao,
                        x.SoLuong1Sao));

                return binhLuans.Select(bl =>
                    MapToView(bl,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].DanhGiaTrungBinh : null,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuongDanhGia : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong5Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong4Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong3Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong2Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong1Sao : 0))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc bình luận theo trạng thái: TrangThai={TrangThai}", trangThai);
                throw;
            }
        }

        public async Task<List<BinhLuanView>> FilterByDanhGiaAsync(int danhGia)
        {
            _logger.LogInformation("Lọc bình luận theo trạng thái: DanhGia={DanhGia}", danhGia);

            try
            {
                var binhLuans = await _context.BinhLuans
                    .AsNoTracking()
                    .Include(bl => bl.Medias)
                    .Include(bl => bl.NguoiDungNavigation)
                    .Include(bl => bl.SanPhamNavigation)
                    .Where(bl => bl.DanhGia == danhGia)
                    .ToListAsync();

                // Calculate rating statistics for each product
                var ratingStats = await _context.BinhLuans
                    .Where(bl => bl.TrangThai == 1 && bl.DanhGia.HasValue)
                    .GroupBy(bl => bl.MaSanPham)
                    .Select(g => new
                    {
                        MaSanPham = g.Key,
                        DanhGiaTrungBinh = (decimal?)g.Average(bl => bl.DanhGia.Value),
                        SoLuongDanhGia = g.Count(),
                        SoLuong5Sao = g.Count(bl => bl.DanhGia == 5),
                        SoLuong4Sao = g.Count(bl => bl.DanhGia == 4),
                        SoLuong3Sao = g.Count(bl => bl.DanhGia == 3),
                        SoLuong2Sao = g.Count(bl => bl.DanhGia == 2),
                        SoLuong1Sao = g.Count(bl => bl.DanhGia == 1)
                    })
                    .ToDictionaryAsync(x => x.MaSanPham, x => (
                        x.DanhGiaTrungBinh,
                        x.SoLuongDanhGia,
                        x.SoLuong5Sao,
                        x.SoLuong4Sao,
                        x.SoLuong3Sao,
                        x.SoLuong2Sao,
                        x.SoLuong1Sao));

                return binhLuans.Select(bl =>
                    MapToView(bl,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].DanhGiaTrungBinh : null,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuongDanhGia : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong5Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong4Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong3Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong2Sao : 0,
                        ratingStats.ContainsKey(bl.MaSanPham ?? 0) ? ratingStats[bl.MaSanPham ?? 0].SoLuong1Sao : 0))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc bình luận theo trạng thái: DanhGia={DanhGia}", danhGia);
                throw;
            }
        }

        private BinhLuanView MapToView(BinhLuan binhLuan, decimal? danhGiaTrungBinh = null, int? soLuongDanhGia = null,
            int? soLuong5Sao = null, int? soLuong4Sao = null, int? soLuong3Sao = null,
            int? soLuong2Sao = null, int? soLuong1Sao = null)
        {
            return new BinhLuanView
            {
                MaBinhLuan = binhLuan.MaBinhLuan,
                TieuDe = binhLuan.TieuDe,
                NoiDung = binhLuan.NoiDung,
                DanhGia = binhLuan.DanhGia,
                NgayTao = binhLuan.NgayTao,
                TrangThai = binhLuan.TrangThai,
                MaNguoiDung = binhLuan.MaNguoiDung,
                HoTen = binhLuan.NguoiDungNavigation?.HoTen,
                Avt = binhLuan.NguoiDungNavigation?.Avt,
                MaSanPham = binhLuan.MaSanPham,
                TenSanPham = binhLuan.SanPhamNavigation?.TenSanPham,
                DanhGiaTrungBinh = danhGiaTrungBinh,
                SoLuongDanhGia = soLuongDanhGia,
                SoLuong5Sao = soLuong5Sao,
                SoLuong4Sao = soLuong4Sao,
                SoLuong3Sao = soLuong3Sao,
                SoLuong2Sao = soLuong2Sao,
                SoLuong1Sao = soLuong1Sao,
                Medias = binhLuan.Medias?.Select(m => new MediaView
                {
                    MaMedia = m.MaMedia,
                    LoaiMedia = m.LoaiMedia,
                    DuongDan = m.DuongDan,
                    AltMedia = m.AltMedia,
                    LinkMedia = m.LinkMedia,
                    NgayTao = m.NgayTao,
                    TrangThai = m.TrangThai,
                    MaSanPham = m.MaSanPham,
                    MaBinhLuan = m.MaBinhLuan
                }).ToList() ?? new List<MediaView>()
            };
        }
    }
}