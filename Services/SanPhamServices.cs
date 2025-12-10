using System;
using System.Collections.Generic;
using System.IO;
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
    public class SanPhamServices : ISanPhamServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediaServices _mediaServices;
        private readonly IMemoryCacheServices _cacheServices;
        private readonly ILogger<SanPhamServices> _logger;

        public SanPhamServices(
            ApplicationDbContext context,
            IMediaServices mediaServices,
            IMemoryCacheServices cacheServices,
            ILogger<SanPhamServices> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaServices = mediaServices ?? throw new ArgumentNullException(nameof(mediaServices));
            _cacheServices = cacheServices ?? throw new ArgumentNullException(nameof(cacheServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        public async Task<SanPhamView> CreateAsync(SanPhamCreate model)
        {
            _logger.LogInformation("Bắt đầu tạo sản phẩm mới: TenSanPham={TenSanPham}, MaLoai={MaLoai}", model.TenSanPham, model.MaLoai);

            if (model == null)
                throw new ArgumentException("Thông tin sản phẩm không hợp lệ.");

            // Sinh slug từ tên sản phẩm
            var baseSlug = GenerateSlug(model.TenSanPham);
            var finalSlug = await GetUniqueSlugAsync(baseSlug);

            // Validate danh mục
            if (!await IsValidDanhMuc(model.MaLoai, 1))
                throw new ArgumentException("Loại sản phẩm không hợp lệ.");

            if (!await IsValidDanhMuc(model.MaThuongHieu, 2))
                throw new ArgumentException("Thương hiệu không hợp lệ.");

            if (model.MaHashtag.HasValue && !await IsValidDanhMuc(model.MaHashtag.Value, 3))
                throw new ArgumentException("Hashtag không hợp lệ.");

            try
            {
                var sanPham = new SanPham
                {
                    TenSanPham = model.TenSanPham,
                    MoTa = model.MoTa,
                    Slug = finalSlug,
                    MaVach = model.MaVach,
                    GioiTinh = model.GioiTinh,
                    MaLoai = model.MaLoai,
                    MaThuongHieu = model.MaThuongHieu,
                    MaHashtag = model.MaHashtag,
                    NgayTao = DateTime.UtcNow,
                    TrangThai = 1
                };

                // Set pricing and stock
                sanPham.GiaBan = model.GiaBan;
                sanPham.GiaSale = model.GiaSale;
                sanPham.SoLuong = model.SoLuong;

                _context.SanPhams.Add(sanPham);
                await _context.SaveChangesAsync();

                // Lưu ảnh
                if (model.Images != null && model.Images.Any())
                {
                    for (int i = 0; i < model.Images.Count; i++)
                    {
                        var imageFile = model.Images[i];

                        if (!imageFile.ContentType.StartsWith("image/"))
                            throw new ArgumentException($"Tệp {imageFile.FileName} không phải hình ảnh.");

                        if (imageFile.Length > 5 * 1024 * 1024)
                            throw new ArgumentException($"Kích thước tệp {imageFile.FileName} không được vượt quá 5MB.");

                        var imageUrl = await _mediaServices.SaveOptimizedImageAsync(imageFile, "sanpham");

                        var media = new Media
                        {
                            MaSanPham = sanPham.MaSanPham,
                            LoaiMedia = "image",
                            DuongDan = imageUrl,
                            AltMedia = $"Ảnh sản phẩm {Truncate(sanPham.TenSanPham, 70)} - {i + 1}",
                            TrangThai = 1,
                            NgayTao = DateTime.UtcNow
                        };
                        _context.Medias.Add(media);
                    }
                    await _context.SaveChangesAsync();
                }

                // Clear cache
                _cacheServices.Remove("SanPham_All");
                _cacheServices.Remove($"SanPham_{sanPham.MaSanPham}");
                _cacheServices.Remove($"SanPham_DanhMuc_{sanPham.MaLoai}");

                return await GetByIdAsync(sanPham.MaSanPham);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sản phẩm: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<SanPhamView> UpdateAsync(int id, SanPhamEdit model, List<IFormFile>? newImageFiles = null)
        {
            _logger.LogInformation("Bắt đầu cập nhật sản phẩm: MaSanPham={Id}, TenSanPham={TenSanPham}", id, model.TenSanPham);

            if (model == null)
            {
                _logger.LogWarning("Thông tin cập nhật không hợp lệ: MaSanPham={Id}", id);
                throw new ArgumentException("Thông tin cập nhật không hợp lệ.");
            }

            try
            {
                var sanPham = await _context.SanPhams
                    .Include(sp => sp.DanhMucLoai)
                    .Include(sp => sp.DanhMucThuongHieu)
                    .Include(sp => sp.DanhMucHashtag)
                    .Include(sp => sp.Medias)
                    .FirstOrDefaultAsync(sp => sp.MaSanPham == id);

                if (sanPham == null)
                {
                    _logger.LogWarning("Sản phẩm không tồn tại: MaSanPham={Id}", id);
                    throw new KeyNotFoundException("Sản phẩm không tồn tại.");
                }

                // Validate input
                if (model.Slug != null && !System.Text.RegularExpressions.Regex.IsMatch(model.Slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
                {
                    _logger.LogWarning("Slug không hợp lệ: Slug={Slug}", model.Slug);
                    throw new ArgumentException("Slug chỉ chứa chữ thường, số và dấu gạch nối.");
                }

                if (model.Slug != null && model.Slug != sanPham.Slug && await _context.SanPhams.AnyAsync(sp => sp.Slug == model.Slug && sp.TrangThai == 1))
                {
                    _logger.LogWarning("Slug đã tồn tại: Slug={Slug}", model.Slug);
                    throw new ArgumentException("Slug đã tồn tại.");
                }

                if (model.MaLoai.HasValue && !await IsValidDanhMuc(model.MaLoai.Value, 1))
                {
                    _logger.LogWarning("Loại sản phẩm không hợp lệ: MaLoai={MaLoai}", model.MaLoai);
                    throw new ArgumentException("Loại sản phẩm không hợp lệ.");
                }

                if (model.MaThuongHieu.HasValue && !await IsValidDanhMuc(model.MaThuongHieu.Value, 2))
                {
                    _logger.LogWarning("Thương hiệu không hợp lệ: MaThuongHieu={MaThuongHieu}", model.MaThuongHieu);
                    throw new ArgumentException("Thương hiệu không hợp lệ.");
                }

                if (model.MaHashtag.HasValue && !await IsValidDanhMuc(model.MaHashtag.Value, 3))
                {
                    _logger.LogWarning("Hashtag không hợp lệ: MaHashtag={MaHashtag}", model.MaHashtag);
                    throw new ArgumentException("Hashtag không hợp lệ.");
                }

                // Update properties
                if (model.TenSanPham != null)
                    sanPham.TenSanPham = model.TenSanPham;
                if (model.MoTa != null)
                    sanPham.MoTa = model.MoTa;
                if (model.Slug != null)
                    sanPham.Slug = model.Slug;
                if (model.MaVach != null)
                    sanPham.MaVach = model.MaVach;
                if (model.GioiTinh.HasValue)
                    sanPham.GioiTinh = model.GioiTinh.Value;
                if (model.MaLoai.HasValue)
                    sanPham.MaLoai = model.MaLoai.Value;
                if (model.MaThuongHieu.HasValue)
                    sanPham.MaThuongHieu = model.MaThuongHieu.Value;
                if (model.MaHashtag.HasValue)
                    sanPham.MaHashtag = model.MaHashtag.Value;
                if (model.TrangThai.HasValue)
                    sanPham.TrangThai = model.TrangThai.Value;

                // Update price and stock if provided
                if (model.GiaBan.HasValue)
                    sanPham.GiaBan = model.GiaBan.Value;

                if (model.GiaSale.HasValue)
                    sanPham.GiaSale = model.GiaSale.Value;

                if (model.SoLuong.HasValue)
                    sanPham.SoLuong = model.SoLuong.Value;

                // Handle new images with proper cleanup
                if (newImageFiles != null && newImageFiles.Any())
                {
                    foreach (var imageFile in newImageFiles)
                    {
                        if (!imageFile.ContentType.StartsWith("image/"))
                        {
                            _logger.LogWarning("Tệp tải lên không phải hình ảnh: {FileName}", imageFile.FileName);
                            throw new ArgumentException($"Tệp {imageFile.FileName} không phải hình ảnh.");
                        }
                        if (imageFile.Length > 5 * 1024 * 1024)
                        {
                            _logger.LogWarning("Kích thước tệp hình ảnh quá lớn: {FileName}, Kích thước: {Size} bytes", imageFile.FileName, imageFile.Length);
                            throw new ArgumentException($"Kích thước tệp {imageFile.FileName} không được vượt quá 5MB.");
                        }

                        var imageUrl = await _mediaServices.SaveOptimizedImageAsync(imageFile, "sanpham");
                        _logger.LogInformation("Hình ảnh mới đã được lưu: {ImageUrl}", imageUrl);

                        var media = new Media
                        {
                            MaSanPham = sanPham.MaSanPham,
                            LoaiMedia = "image",
                            DuongDan = imageUrl,
                            AltMedia = $"Ảnh sản phẩm {Truncate(sanPham.TenSanPham, 70)} - {sanPham.Medias.Count + 1}",
                            TrangThai = 1,
                            NgayTao = DateTime.UtcNow
                        };
                        _context.Medias.Add(media);
                    }

                    _logger.LogInformation("Đã thêm {Count} hình ảnh mới cho sản phẩm: MaSanPham={Id}", newImageFiles.Count, id);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Sản phẩm được cập nhật thành công: MaSanPham={Id}", id);

                // Clear cache
                _cacheServices.Remove($"SanPham_{id}");
                _cacheServices.Remove("SanPham_All");
                _cacheServices.Remove($"SanPham_DanhMuc_{sanPham.MaLoai}");

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm: MaSanPham={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, bool hardDeleteImages = false)
        {
            _logger.LogInformation("Bắt đầu xóa sản phẩm: MaSanPham={Id}, HardDeleteImages={HardDeleteImages}",
                id, hardDeleteImages);

            try
            {
                var sanPham = await _context.SanPhams
                    .Include(sp => sp.Medias)
                    .FirstOrDefaultAsync(sp => sp.MaSanPham == id);

                if (sanPham == null)
                {
                    _logger.LogWarning("Sản phẩm không tồn tại: MaSanPham={Id}", id);
                    return false;
                }

                // Xử lý media
                if (hardDeleteImages && sanPham.Medias.Any())
                {
                    // Hard delete - xóa file vật lý và record
                    foreach (var media in sanPham.Medias.ToList())
                    {
                        await _mediaServices.DeleteImageAsync(media.DuongDan);
                        _context.Medias.Remove(media);
                    }
                    _logger.LogInformation("Đã hard delete {Count} hình ảnh", sanPham.Medias.Count);
                }
                else
                {
                    // Soft delete - chỉ đánh dấu
                    foreach (var media in sanPham.Medias)
                    {
                        media.TrangThai = 0;
                    }
                }

                // Soft delete sản phẩm
                sanPham.TrangThai = 0;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Sản phẩm được xóa thành công: MaSanPham={Id}", id);

                // Clear cache
                _cacheServices.Remove($"SanPham_{id}");
                _cacheServices.Remove("SanPham_All");
                _cacheServices.Remove($"SanPham_DanhMuc_{sanPham.MaLoai}");
                _cacheServices.Remove("SanPham_BestSelling");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm: MaSanPham={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        public async Task<SanPhamView> GetByIdAsync(int id)
        {
            _logger.LogInformation("Truy vấn sản phẩm: MaSanPham={Id}", id);

            try
            {
                return await _cacheServices.GetOrCreateAsync($"SanPham_{id}", async () =>
                {
                    var sanPham = await _context.SanPhams
                        .AsNoTracking()
                        .Include(sp => sp.DanhMucLoai)
                        .Include(sp => sp.DanhMucThuongHieu)
                        .Include(sp => sp.DanhMucHashtag)
                        .Include(sp => sp.Medias)
                        .Include(sp => sp.BinhLuans)
                        .FirstOrDefaultAsync(sp => sp.MaSanPham == id);

                    if (sanPham == null)
                    {
                        _logger.LogWarning("Không tìm thấy sản phẩm: MaSanPham={Id}", id);
                        return null;
                    }

                    return MapToView(sanPham);
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn sản phẩm: MaSanPham={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        public async Task<List<SanPhamView>> GetAllAsync()
        {
            _logger.LogInformation("Truy vấn tất cả sản phẩm");

            try
            {
                return await _cacheServices.GetOrCreateAsync("SanPham_All", async () =>
                {
                    var sanPhams = await _context.SanPhams
                        .AsNoTracking()
                        .Include(sp => sp.DanhMucLoai)
                        .Include(sp => sp.DanhMucThuongHieu)
                        .Include(sp => sp.DanhMucHashtag)
                        .Include(sp => sp.Medias)
                        .Include(sp => sp.BinhLuans)
                        .OrderByDescending(sp => sp.NgayTao)
                        .ToListAsync();

                    return sanPhams.Select(MapToView).ToList();
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn tất cả sản phẩm, StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        public async Task<List<SanPhamView>> SearchAsync(int? trangThai, int? maSanPham, string? tenSanPham)
        {
            _logger.LogInformation("Tìm kiếm sản phẩm: TrangThai={TrangThai}, MaSanPham={MaSanPham}, TenSanPham={TenSanPham}",
                trangThai, maSanPham, tenSanPham);

            try
            {
                var query = _context.SanPhams
                    .AsNoTracking()
                    .Include(sp => sp.DanhMucLoai)
                    .Include(sp => sp.DanhMucThuongHieu)
                    .Include(sp => sp.DanhMucHashtag)
                    .Include(sp => sp.Medias)
                    .Include(sp => sp.BinhLuans)
                    .AsQueryable();

                if (trangThai.HasValue)
                {
                    query = query.Where(sp => sp.TrangThai == trangThai.Value);
                }

                if (maSanPham.HasValue)
                {
                    query = query.Where(sp => sp.MaSanPham == maSanPham.Value);
                }

                if (!string.IsNullOrEmpty(tenSanPham))
                {
                    query = query.Where(sp => sp.TenSanPham.Contains(tenSanPham));
                }

                var sanPhams = await query
                    .OrderByDescending(sp => sp.NgayTao)
                    .ThenBy(sp => sp.TenSanPham) // Sắp theo tên A-Z nếu cùng NgayTao
                    .ToListAsync();


                return sanPhams.Select(MapToView).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm sản phẩm, StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        public async Task<List<SanPhamView>> FilterByLoaiDanhMucAsync(int maLoaiDanhMuc)
        {
            _logger.LogInformation("Lọc sản phẩm theo loại danh mục: MaLoaiDanhMuc={MaLoaiDanhMuc}", maLoaiDanhMuc);

            try
            {
                var danhMuc = await _context.DanhMucs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dm => dm.MaDanhMuc == maLoaiDanhMuc && dm.TrangThai == 1);

                if (danhMuc == null)
                {
                    _logger.LogWarning("Danh mục không tồn tại hoặc đã bị vô hiệu hóa: MaLoaiDanhMuc={MaLoaiDanhMuc}", maLoaiDanhMuc);
                    return new List<SanPhamView>();
                }

                var query = _context.SanPhams
                    .AsNoTracking()
                    .Include(sp => sp.DanhMucLoai)
                    .Include(sp => sp.DanhMucThuongHieu)
                    .Include(sp => sp.DanhMucHashtag)
                    .Include(sp => sp.Medias)
                    .Include(sp => sp.BinhLuans)
                    .Where(sp => sp.TrangThai == 1);

                if (danhMuc.LoaiDanhMuc == 1)
                {
                    query = query.Where(sp => sp.MaLoai == maLoaiDanhMuc);
                }
                else if (danhMuc.LoaiDanhMuc == 2)
                {
                    query = query.Where(sp => sp.MaThuongHieu == maLoaiDanhMuc);
                }
                else if (danhMuc.LoaiDanhMuc == 3)
                {
                    query = query.Where(sp => sp.MaHashtag == maLoaiDanhMuc);
                }
                else
                {
                    _logger.LogWarning("Loại danh mục không hợp lệ cho sản phẩm: MaLoaiDanhMuc={MaLoaiDanhMuc}, LoaiDanhMuc={LoaiDanhMuc}", maLoaiDanhMuc, danhMuc.LoaiDanhMuc);
                    return new List<SanPhamView>();
                }

                var sanPhams = await query
                    .OrderByDescending(sp => sp.NgayTao) // 🔥 Sắp xếp mới nhất lên đầu
                    .ToListAsync();

                return sanPhams.Select(MapToView).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc sản phẩm theo loại danh mục: MaLoaiDanhMuc={MaLoaiDanhMuc}, StackTrace: {StackTrace}", maLoaiDanhMuc, ex.StackTrace);
                throw;
            }
        }

        public async Task<List<SanPhamView>> GetByDanhMucAsync(int maDanhMuc)
        {
            _logger.LogInformation("Lấy sản phẩm theo danh mục: MaDanhMuc={MaDanhMuc}", maDanhMuc);

            try
            {
                return await _cacheServices.GetOrCreateAsync($"SanPham_DanhMuc_{maDanhMuc}", async () =>
                {
                    var danhMuc = await _context.DanhMucs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(dm => dm.MaDanhMuc == maDanhMuc && dm.TrangThai == 1);

                    if (danhMuc == null)
                    {
                        _logger.LogWarning("Danh mục không tồn tại hoặc đã bị vô hiệu hóa: MaDanhMuc={MaDanhMuc}", maDanhMuc);
                        return new List<SanPhamView>();
                    }

                    var query = _context.SanPhams
                        .AsNoTracking()
                        .Include(sp => sp.DanhMucLoai)
                        .Include(sp => sp.DanhMucThuongHieu)
                        .Include(sp => sp.DanhMucHashtag)
                        .Include(sp => sp.Medias)
                        .Include(sp => sp.BinhLuans)
                        .Where(sp => sp.TrangThai == 1);

                    if (danhMuc.LoaiDanhMuc == 1)
                    {
                        query = query.Where(sp => sp.MaLoai == maDanhMuc);
                    }
                    else if (danhMuc.LoaiDanhMuc == 2)
                    {
                        query = query.Where(sp => sp.MaThuongHieu == maDanhMuc);
                    }
                    else if (danhMuc.LoaiDanhMuc == 3)
                    {
                        query = query.Where(sp => sp.MaHashtag == maDanhMuc);
                    }
                    else
                    {
                        _logger.LogWarning("Loại danh mục không hợp lệ: MaDanhMuc={MaDanhMuc}, LoaiDanhMuc={LoaiDanhMuc}", maDanhMuc, danhMuc.LoaiDanhMuc);
                        return new List<SanPhamView>();
                    }

                    var sanPhams = await query
                        .OrderByDescending(sp => sp.NgayTao) // 🔥 Sắp xếp sản phẩm mới nhất lên đầu
                        .ToListAsync();

                    return sanPhams.Select(MapToView).ToList();

                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm theo danh mục: MaDanhMuc={MaDanhMuc}, StackTrace: {StackTrace}", maDanhMuc, ex.StackTrace);
                throw;
            }
        }

        public async Task<List<SanPhamView>> GetBestSellingAsync(int limit)
        {
            _logger.LogInformation("Lấy danh sách sản phẩm bán chạy, Giới hạn: {Limit}", limit);

            try
            {
                return await _cacheServices.GetOrCreateAsync($"SanPham_BestSelling_{limit}", async () =>
                {
                    var sanPhams = await _context.SanPhams
                        .AsNoTracking()
                        .Include(sp => sp.DanhMucLoai)
                        .Include(sp => sp.DanhMucThuongHieu)
                        .Include(sp => sp.DanhMucHashtag)
                        .Include(sp => sp.Medias)
                        .Include(sp => sp.BinhLuans)
                        .Where(sp => sp.TrangThai == 1)
                        // Order by sale price descending (if GiaSale is null, fallback to GiaBan), then by newest
                        .OrderByDescending(sp => (sp.GiaSale ?? sp.GiaBan))
                        .ThenByDescending(sp => sp.NgayTao)
                        .Take(limit)
                        .ToListAsync();

                    return sanPhams.Select(MapToView).ToList();
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm bán chạy, StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        private async Task<bool> IsValidDanhMuc(int maDanhMuc, int loaiDanhMuc)
        {
            return await _context.DanhMucs
                .AsNoTracking()
                .AnyAsync(dm => dm.MaDanhMuc == maDanhMuc && dm.LoaiDanhMuc == loaiDanhMuc && dm.TrangThai == 1);
        }

        private string GenerateSlug(string input)
        {
            string slug = input.ToLowerInvariant();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim('-');
            slug = System.Text.RegularExpressions.Regex.Replace(slug, "-+", "-");
            return slug;
        }

        private async Task<string> GetUniqueSlugAsync(string baseSlug)
        {
            string slug = baseSlug;
            int count = 1;

            while (await _context.SanPhams.AnyAsync(sp => sp.Slug == slug && sp.TrangThai == 1))
            {
                slug = $"{baseSlug}-{count}";
                count++;
            }

            return slug;
        }

        public async Task<List<SanPhamView>> GetByLoaiAndThuongHieuAsync(int maLoai, int maThuongHieu)
        {
            _logger.LogInformation("Lấy sản phẩm theo loại và thương hiệu: MaLoai={MaLoai}, MaThuongHieu={MaThuongHieu}", maLoai, maThuongHieu);

            try
            {
                return await _cacheServices.GetOrCreateAsync($"SanPham_Loai_ThuongHieu_{maLoai}_{maThuongHieu}", async () =>
                {
                    // Validate danh mục
                    var loaiExists = await _context.DanhMucs
                        .AsNoTracking()
                        .AnyAsync(dm => dm.MaDanhMuc == maLoai && dm.LoaiDanhMuc == 1 && dm.TrangThai == 1);
                    var thuongHieuExists = await _context.DanhMucs
                        .AsNoTracking()
                        .AnyAsync(dm => dm.MaDanhMuc == maThuongHieu && dm.LoaiDanhMuc == 2 && dm.TrangThai == 1);

                    if (!loaiExists || !thuongHieuExists)
                    {
                        _logger.LogWarning("Loại sản phẩm hoặc thương hiệu không hợp lệ: MaLoai={MaLoai}, MaThuongHieu={MaThuongHieu}", maLoai, maThuongHieu);
                        return new List<SanPhamView>();
                    }

                    var sanPhams = await _context.SanPhams
                        .AsNoTracking()
                        .Include(sp => sp.DanhMucLoai)
                        .Include(sp => sp.DanhMucThuongHieu)
                        .Include(sp => sp.DanhMucHashtag)
                        .Include(sp => sp.Medias)
                        .Include(sp => sp.BinhLuans)
                        .Where(sp => sp.TrangThai == 1 && sp.MaLoai == maLoai && sp.MaThuongHieu == maThuongHieu)
                        .OrderByDescending(sp => sp.NgayTao) // 🔥 Sắp xếp mới nhất lên đầu
                        .ToListAsync();


                    return sanPhams.Select(MapToView).ToList();
                }, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm theo loại và thương hiệu: MaLoai={MaLoai}, MaThuongHieu={MaThuongHieu}, StackTrace: {StackTrace}",
                    maLoai, maThuongHieu, ex.StackTrace);
                throw;
            }
        }

        public async Task<bool> DeleteProductImageAsync(int productId, int mediaId, bool hardDelete = false)
        {
            _logger.LogInformation("Bắt đầu xóa hình ảnh sản phẩm: ProductId={ProductId}, MediaId={MediaId}, HardDelete={HardDelete}",
                productId, mediaId, hardDelete);

            try
            {
                var media = await _context.Medias
                    .FirstOrDefaultAsync(m => m.MaMedia == mediaId && m.MaSanPham == productId);

                if (media == null)
                {
                    _logger.LogWarning("Không tìm thấy hình ảnh: ProductId={ProductId}, MediaId={MediaId}", productId, mediaId);
                    return false;
                }

                if (hardDelete)
                {
                    // Xóa file vật lý
                    var deleteFileResult = await _mediaServices.DeleteImageAsync(media.DuongDan);
                    if (deleteFileResult)
                    {
                        _logger.LogInformation("Đã xóa file vật lý: {FilePath}", media.DuongDan);
                    }
                    else
                    {
                        _logger.LogWarning("Không thể xóa file vật lý: {FilePath}", media.DuongDan);
                    }

                    // Xóa record khỏi database
                    _context.Medias.Remove(media);
                    _logger.LogInformation("Đã xóa record khỏi database: MediaId={MediaId}", mediaId);
                }
                else
                {
                    // Soft delete - chỉ cập nhật trạng thái
                    media.TrangThai = 0;
                    _logger.LogInformation("Đã soft delete hình ảnh: MediaId={MediaId}", mediaId);
                }

                await _context.SaveChangesAsync();

                // Clear cache
                _cacheServices.Remove($"SanPham_{productId}");
                _cacheServices.Remove("SanPham_All");

                _logger.LogInformation("Xóa hình ảnh sản phẩm thành công: ProductId={ProductId}, MediaId={MediaId}",
                    productId, mediaId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hình ảnh sản phẩm: ProductId={ProductId}, MediaId={MediaId}, StackTrace: {StackTrace}",
                    productId, mediaId, ex.StackTrace);
                throw;
            }
        }

        public async Task<BatchDeleteResult> DeleteProductImagesAsync(int productId, List<int> mediaIds, bool hardDelete = false)
        {
            var result = new BatchDeleteResult
            {
                TotalRequested = mediaIds?.Count ?? 0
            };

            if (mediaIds == null || !mediaIds.Any())
            {
                result.Message = "Danh sách hình ảnh trống.";
                return result;
            }

            _logger.LogInformation("Bắt đầu xóa batch hình ảnh: ProductId={ProductId}, Count={Count}, HardDelete={HardDelete}",
                productId, mediaIds.Count, hardDelete);

            foreach (var mediaId in mediaIds)
            {
                try
                {
                    var success = await DeleteProductImageAsync(productId, mediaId, hardDelete);

                    if (success)
                    {
                        result.SuccessCount++;
                        result.SuccessfulIds.Add(mediaId);
                    }
                    else
                    {
                        result.FailedCount++;
                        result.FailedIds.Add(mediaId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xóa hình ảnh trong batch: MediaId={MediaId}", mediaId);
                    result.FailedCount++;
                    result.FailedIds.Add(mediaId);
                }
            }

            result.Message = $"Xóa thành công {result.SuccessCount}/{result.TotalRequested} hình ảnh.";

            _logger.LogInformation("Hoàn thành xóa batch: Success={Success}, Failed={Failed}",
                result.SuccessCount, result.FailedCount);

            return result;
        }

        private SanPhamView MapToView(SanPham sanPham)
        {
            return new SanPhamView
            {
                MaSanPham = sanPham.MaSanPham,
                TenSanPham = sanPham.TenSanPham,
                MoTa = sanPham.MoTa,
                Slug = sanPham.Slug,
                MaVach = sanPham.MaVach,
                NgayTao = sanPham.NgayTao,
                TrangThai = sanPham.TrangThai,
                GioiTinh = sanPham.GioiTinh,
                MaLoai = sanPham.MaLoai,
                TenLoai = sanPham.DanhMucLoai?.TenDanhMuc,
                HinhAnhLoai = sanPham.DanhMucLoai?.HinhAnh,
                MaThuongHieu = sanPham.MaThuongHieu,
                TenThuongHieu = sanPham.DanhMucThuongHieu?.TenDanhMuc,
                HinhAnhThuongHieu = sanPham.DanhMucThuongHieu?.HinhAnh,
                MaHashtag = sanPham.MaHashtag,
                TenHashtag = sanPham.DanhMucHashtag?.TenDanhMuc,
                HinhAnhHashtag = sanPham.DanhMucHashtag?.HinhAnh,
                Medias = sanPham.Medias?.Select(m => new MediaView
                {
                    MaMedia = m.MaMedia,
                    MaSanPham = m.MaSanPham,
                    TenSanPham = sanPham.TenSanPham,
                    LoaiMedia = m.LoaiMedia,
                    DuongDan = m.DuongDan,
                    AltMedia = m.AltMedia,
                    TrangThai = m.TrangThai,
                    NgayTao = m.NgayTao
                }).ToList() ?? new List<MediaView>(),
                SoLuongDanhGia = sanPham.BinhLuans != null ? sanPham.BinhLuans.Count(bl => bl.DanhGia.HasValue) : 0,
                DanhGiaTrungBinh = sanPham.BinhLuans != null && sanPham.BinhLuans.Any(bl => bl.DanhGia.HasValue)
                ? (decimal?)Convert.ToDecimal(
                    sanPham.BinhLuans
                        .Where(bl => bl.DanhGia.HasValue)
                        .Average(bl => (double)bl.DanhGia.Value)
                    )
                : null
                ,
                GiaBan = sanPham.GiaBan,
                GiaSale = sanPham.GiaSale,
                SoLuong = sanPham.SoLuong,
                // Calculate sale percent and price after sale
                PhanTramSale = (sanPham.GiaBan > 0 && sanPham.GiaSale.HasValue)
                    ? Math.Round((sanPham.GiaSale.Value / sanPham.GiaBan) * 100m, 2)
                    : (decimal?)null,
                GiaSauSale = Math.Max(0m, sanPham.GiaBan - (sanPham.GiaSale ?? 0m))
            };
        }
    }
}
