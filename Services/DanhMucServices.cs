using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    public class DanhMucServices : IDanhMucServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediaServices _mediaServices;
        private readonly IMemoryCacheServices _cacheServices;
        private readonly ILogger<DanhMucServices> _logger;

        public DanhMucServices(
            ApplicationDbContext context,
            IMediaServices mediaServices,
            IMemoryCacheServices cacheServices,
            ILogger<DanhMucServices> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaServices = mediaServices ?? throw new ArgumentNullException(nameof(mediaServices));
            _cacheServices = cacheServices ?? throw new ArgumentNullException(nameof(cacheServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DanhMucView> CreateAsync(DanhMucCreate model, IFormFile imageFile)
        {
            _logger.LogInformation("Bắt đầu tạo danh mục mới: {TenDanhMuc}, LoaiDanhMuc: {LoaiDanhMuc}",
                model?.TenDanhMuc, model?.LoaiDanhMuc);

            if (model == null || string.IsNullOrWhiteSpace(model.TenDanhMuc))
            {
                _logger.LogWarning("Thông tin danh mục không hợp lệ: {TenDanhMuc}", model?.TenDanhMuc);
                throw new ArgumentException("Thông tin danh mục không hợp lệ.");
            }

            try
            {
                var exists = await _context.DanhMucs
                    .AsNoTracking()
                    .AnyAsync(d => d.TenDanhMuc == model.TenDanhMuc && d.LoaiDanhMuc == model.LoaiDanhMuc);
                if (exists)
                {
                    _logger.LogWarning("Tên danh mục '{TenDanhMuc}' đã tồn tại trong loại danh mục {LoaiDanhMuc}.",
                        model.TenDanhMuc, model.LoaiDanhMuc);
                    throw new InvalidOperationException($"Tên danh mục '{model.TenDanhMuc}' đã tồn tại.");
                }

                string imageUrl = null;
                if (imageFile != null)
                {
                    if (!imageFile.ContentType.StartsWith("image/"))
                    {
                        _logger.LogWarning("Tệp tải lên không phải hình ảnh: {FileName}", imageFile.FileName);
                        throw new ArgumentException("Tệp không phải hình ảnh.");
                    }
                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        _logger.LogWarning("Kích thước tệp hình ảnh quá lớn: {FileName}, Kích thước: {Size} bytes", imageFile.FileName, imageFile.Length);
                        throw new ArgumentException("Kích thước tệp hình ảnh không được vượt quá 5MB.");
                    }
                    _logger.LogInformation("Đang xử lý hình ảnh: {FileName}", imageFile.FileName);
                    imageUrl = await _mediaServices.SaveOptimizedImageAsync(imageFile, "danhmuc");
                    _logger.LogInformation("Hình ảnh đã được lưu: {ImageUrl}", imageUrl);
                }

                var danhMuc = new DanhMuc
                {
                    TenDanhMuc = model.TenDanhMuc,
                    LoaiDanhMuc = model.LoaiDanhMuc,
                    HinhAnh = imageUrl,
                    TrangThai = model.TrangThai,
                    NgayTao = DateTime.UtcNow
                };

                _context.DanhMucs.Add(danhMuc);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Danh mục được tạo thành công: MaDanhMuc={MaDanhMuc}", danhMuc.MaDanhMuc);

                _cacheServices.Remove("DanhMuc_All");
                _cacheServices.Remove($"DanhMuc_{danhMuc.MaDanhMuc}");
                _cacheServices.Remove($"DanhMuc_CategoryType_{danhMuc.LoaiDanhMuc}");
                _logger.LogDebug("Đã xóa cache: DanhMuc_All, DanhMuc_{MaDanhMuc}", danhMuc.MaDanhMuc);

                return MapToView(danhMuc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo danh mục: {TenDanhMuc}, StackTrace: {StackTrace}", model.TenDanhMuc, ex.StackTrace);
                throw;
            }
        }

        public async Task<DanhMucView> UpdateAsync(int id, DanhMucEdit model, IFormFile imageFile = null)
        {
            _logger.LogInformation("Bắt đầu cập nhật danh mục: MaDanhMuc={Id}, TenDanhMuc={TenDanhMuc}",
                id, model?.TenDanhMuc);

            if (model == null)
            {
                _logger.LogWarning("Thông tin cập nhật không hợp lệ: MaDanhMuc={Id}", id);
                throw new ArgumentException("Thông tin cập nhật không hợp lệ.");
            }

            try
            {
                var danhMuc = await _context.DanhMucs.FindAsync(id);
                if (danhMuc == null)
                {
                    _logger.LogWarning("Danh mục không tồn tại: MaDanhMuc={Id}", id);
                    throw new KeyNotFoundException("Danh mục không tồn tại.");
                }

                if (!string.IsNullOrEmpty(model.TenDanhMuc) && model.TenDanhMuc != danhMuc.TenDanhMuc)
                {
                    var exists = await _context.DanhMucs
                        .AsNoTracking()
                        .AnyAsync(d => d.TenDanhMuc == model.TenDanhMuc && d.LoaiDanhMuc == danhMuc.LoaiDanhMuc && d.MaDanhMuc != id);
                    if (exists)
                    {
                        _logger.LogWarning("Tên danh mục '{TenDanhMuc}' đã tồn tại trong loại danh mục {LoaiDanhMuc}.",
                            model.TenDanhMuc, danhMuc.LoaiDanhMuc);
                        throw new InvalidOperationException($"Tên danh mục '{model.TenDanhMuc}' đã tồn tại.");
                    }

                    danhMuc.TenDanhMuc = model.TenDanhMuc;
                    _logger.LogInformation("Cập nhật tên danh mục: {TenDanhMuc}", model.TenDanhMuc);
                }

                if (imageFile != null)
                {
                    if (!imageFile.ContentType.StartsWith("image/"))
                    {
                        _logger.LogWarning("Tệp tải lên không phải hình ảnh: {FileName}", imageFile.FileName);
                        throw new ArgumentException("Tệp không phải hình ảnh.");
                    }
                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        _logger.LogWarning("Kích thước tệp hình ảnh quá lớn: {FileName}, Kích thước: {Size} bytes", imageFile.FileName, imageFile.Length);
                        throw new ArgumentException("Kích thước tệp hình ảnh không được vượt quá 5MB.");
                    }

                    if (!string.IsNullOrEmpty(danhMuc.HinhAnh))
                    {
                        var oldImageDeleted = await _mediaServices.DeleteImageAsync(danhMuc.HinhAnh);
                        if (oldImageDeleted)
                        {
                            _logger.LogInformation("Đã xóa hình ảnh cũ: {OldImageUrl}", danhMuc.HinhAnh);
                        }
                        else
                        {
                            _logger.LogWarning("Không thể xóa hình ảnh cũ: {OldImageUrl}", danhMuc.HinhAnh);
                        }
                    }

                    danhMuc.HinhAnh = await _mediaServices.SaveOptimizedImageAsync(imageFile, "danhmuc");
                    _logger.LogInformation("Hình ảnh mới được lưu: {ImageUrl}", danhMuc.HinhAnh);
                }
                else if (!string.IsNullOrEmpty(model.HinhAnh))
                {
                    danhMuc.HinhAnh = model.HinhAnh;
                    _logger.LogInformation("Cập nhật đường dẫn hình ảnh: {HinhAnh}", model.HinhAnh);
                }

                if (model.LoaiDanhMuc.HasValue && model.LoaiDanhMuc != danhMuc.LoaiDanhMuc)
                {
                    _logger.LogWarning("Không được phép thay đổi LoaiDanhMuc: {NewLoaiDanhMuc}, Hiện tại: {OldLoaiDanhMuc}",
                        model.LoaiDanhMuc, danhMuc.LoaiDanhMuc);
                }

                if (model.TrangThai.HasValue)
                {
                    danhMuc.TrangThai = model.TrangThai.Value;
                    _logger.LogInformation("Cập nhật trạng thái: {TrangThai}", model.TrangThai);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Danh mục được cập nhật thành công: MaDanhMuc={Id}", id);

                _cacheServices.Remove("DanhMuc_All");
                _cacheServices.Remove($"DanhMuc_{id}");
                _cacheServices.Remove($"DanhMuc_Search_{model.TenDanhMuc}");
                _cacheServices.Remove($"DanhMuc_Status_{danhMuc.TrangThai}");
                _cacheServices.Remove($"DanhMuc_CategoryType_{danhMuc.LoaiDanhMuc}");
                _logger.LogDebug("Đã xóa cache: MaDanhMuc={Id}", id);

                return MapToView(danhMuc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật danh mục: MaDanhMuc={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Bắt đầu xóa danh mục: MaDanhMuc={Id}", id);

            try
            {
                var danhMuc = await _context.DanhMucs.FindAsync(id);
                if (danhMuc == null)
                {
                    _logger.LogWarning("Danh mục không tồn tại: MaDanhMuc={Id}", id);
                    return false;
                }

                var isUsedInSanPham = await _context.SanPhams
                    .AsNoTracking()
                    .AnyAsync(sp => sp.MaLoai == id || sp.MaThuongHieu == id || sp.MaHashtag == id);

                if (isUsedInSanPham)
                {
                    _logger.LogWarning("Không thể xóa danh mục vì đang được sử dụng trong SanPham: MaDanhMuc={Id}", id);
                    throw new InvalidOperationException("Danh mục đang được sử dụng trong sản phẩm.");
                }

                if (!string.IsNullOrEmpty(danhMuc.HinhAnh))
                {
                    var imageDeleted = await _mediaServices.DeleteImageAsync(danhMuc.HinhAnh);
                    if (imageDeleted)
                    {
                        _logger.LogInformation("Đã xóa hình ảnh danh mục: {ImageUrl}", danhMuc.HinhAnh);
                    }
                    else
                    {
                        _logger.LogWarning("Không thể xóa hình ảnh danh mục: {ImageUrl}", danhMuc.HinhAnh);
                    }
                }

                _context.DanhMucs.Remove(danhMuc);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Danh mục được xóa thành công: MaDanhMuc={Id}", id);

                _cacheServices.Remove("DanhMuc_All");
                _cacheServices.Remove($"DanhMuc_{id}");
                _cacheServices.Remove($"DanhMuc_Status_{danhMuc.TrangThai}");
                _cacheServices.Remove($"DanhMuc_CategoryType_{danhMuc.LoaiDanhMuc}");
                _logger.LogDebug("Đã xóa cache: MaDanhMuc={Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa danh mục: MaDanhMuc={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        public async Task<DanhMucView> GetByIdAsync(int id)
        {
            _logger.LogInformation("Truy vấn danh mục: MaDanhMuc={Id}", id);

            try
            {
                var result = await _cacheServices.GetOrCreateAsync($"DanhMuc_{id}", async () =>
                {
                    var danhMuc = await _context.DanhMucs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.MaDanhMuc == id);
                    if (danhMuc == null)
                    {
                        _logger.LogWarning("Không tìm thấy danh mục: MaDanhMuc={Id}", id);
                        return null;
                    }
                    _logger.LogInformation("Lấy danh mục từ cơ sở dữ liệu: MaDanhMuc={Id}", id);
                    return MapToView(danhMuc);
                }, TimeSpan.FromMinutes(15));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn danh mục: MaDanhMuc={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        public async Task<IEnumerable<DanhMucView>> GetAllAsync()
        {
            _logger.LogInformation("Truy vấn tất cả danh mục");

            try
            {
                var result = await _cacheServices.GetOrCreateAsync("DanhMuc_All", async () =>
                {
                    var danhMucs = await _context.DanhMucs
                        .AsNoTracking()
                        .OrderByDescending(d => d.NgayTao) // Sắp xếp mới nhất lên đầu
                        .Select(d => MapToView(d))
                        .ToListAsync();

                    _logger.LogInformation("Lấy {Count} danh mục từ cơ sở dữ liệu", danhMucs.Count);
                    return danhMucs;
                }, TimeSpan.FromMinutes(15));

                _logger.LogDebug("Lấy {Count} danh mục từ cache hoặc cơ sở dữ liệu", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn tất cả danh mục, StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }


        public async Task<IEnumerable<DanhMucView>> SearchAsync(string keyword)
        {
            _logger.LogInformation("Tìm kiếm danh mục với từ khóa: {Keyword}", keyword);

            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    _logger.LogDebug("Từ khóa trống, trả về tất cả danh mục");
                    return await GetAllAsync();
                }

                var normalizedKeyword = keyword.Normalize(NormalizationForm.FormD).ToLowerInvariant();
                var cacheKey = $"DanhMuc_Search_{normalizedKeyword.Replace(" ", "_")}";

                var result = await _cacheServices.GetOrCreateAsync(cacheKey, async () =>
                {
                    var danhMucs = await _context.DanhMucs
                        .AsNoTracking()
                        .Where(d => EF.Functions.Like(
                            EF.Functions.Collate(d.TenDanhMuc, "SQL_Latin1_General_CP1_CI_AI"),
                            $"%{normalizedKeyword}%"))
                        .Select(d => MapToView(d))
                        .ToListAsync();
                    _logger.LogInformation("Tìm thấy {Count} danh mục với từ khóa: {Keyword}", danhMucs.Count, keyword);
                    return danhMucs;
                }, TimeSpan.FromMinutes(10));

                _logger.LogDebug("Tìm kiếm trả về {Count} danh mục từ cache hoặc cơ sở dữ liệu", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm danh mục với từ khóa: {Keyword}, StackTrace: {StackTrace}", keyword, ex.StackTrace);
                throw;
            }
        }

        public async Task<IEnumerable<DanhMucView>> FilterByStatusAsync(int status)
        {
            _logger.LogInformation("Lọc danh mục theo trạng thái: {Status}", status);

            try
            {
                var result = await _cacheServices.GetOrCreateAsync($"DanhMuc_Status_{status}", async () =>
                {
                    var danhMucs = await _context.DanhMucs
                        .AsNoTracking()
                        .Where(d => d.TrangThai == status)
                        .Select(d => MapToView(d))
                        .ToListAsync();
                    _logger.LogInformation("Lọc được {Count} danh mục với trạng thái: {Status}", danhMucs.Count, status);
                    return danhMucs;
                }, TimeSpan.FromMinutes(15));

                _logger.LogDebug("Lọc trạng thái trả về {Count} danh mục từ cache hoặc cơ sở dữ liệu", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc danh mục theo trạng thái: {Status}, StackTrace: {StackTrace}", status, ex.StackTrace);
                throw;
            }
        }

        public async Task<IEnumerable<DanhMucView>> FilterByCategoryTypeAsync(int loaiDanhMuc)
        {
            _logger.LogInformation("Lọc danh mục theo loại: {LoaiDanhMuc}", loaiDanhMuc);

            try
            {
                var result = await _cacheServices.GetOrCreateAsync($"DanhMuc_CategoryType_{loaiDanhMuc}", async () =>
                {
                    var danhMucs = await _context.DanhMucs
                        .AsNoTracking()
                        .Where(d => d.LoaiDanhMuc == loaiDanhMuc)
                        .Select(d => MapToView(d))
                        .ToListAsync();
                    _logger.LogInformation("Lọc được {Count} danh mục với loại: {LoaiDanhMuc}", danhMucs.Count, loaiDanhMuc);
                    return danhMucs;
                }, TimeSpan.FromMinutes(15));

                _logger.LogDebug("Lọc loại danh mục trả về {Count} danh mục từ cache hoặc cơ sở dữ liệu", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc danh mục theo loại: {LoaiDanhMuc}, StackTrace: {StackTrace}", loaiDanhMuc, ex.StackTrace);
                throw;
            }
        }

        private static DanhMucView MapToView(DanhMuc danhMuc)
        {
            return new DanhMucView
            {
                MaDanhMuc = danhMuc.MaDanhMuc,
                TenDanhMuc = danhMuc.TenDanhMuc,
                LoaiDanhMuc = danhMuc.LoaiDanhMuc,
                HinhAnh = danhMuc.HinhAnh,
                TrangThai = danhMuc.TrangThai,
                NgayTao = danhMuc.NgayTao
            };
        }
    }
}
