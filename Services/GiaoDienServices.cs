using FashionApi.DTO;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FashionApi.Services
{
    public class GiaoDienServices : IGiaoDienServices
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly IMemoryCacheServices _cache;
        private readonly IMediaServices _mediaServices;
        private readonly ILogger<GiaoDienServices> _logger;

        public GiaoDienServices(
            Data.ApplicationDbContext context,
            IMemoryCacheServices cache,
            IMediaServices mediaServices,
            ILogger<GiaoDienServices> logger)
        {
            _context = context;
            _cache = cache;
            _mediaServices = mediaServices;
            _logger = logger;
        }

        public async Task<IEnumerable<GiaoDienView>> GetAllAsync()
        {
            return await _cache.GetOrCreateAsync("GiaoDien_GetAll", async () =>
            {
                var giaoDiens = await _context.GiaoDiens
                    .Include(gd => gd.Media)
                    .OrderBy(gd => gd.MaGiaoDien)
                    .ToListAsync();

                return giaoDiens.Select(gd => new GiaoDienView
                {
                    MaGiaoDien = gd.MaGiaoDien,
                    TenGiaoDien = gd.TenGiaoDien,
                    LoaiGiaoDien = gd.LoaiGiaoDien,
                    MoTa = gd.MoTa,
                    MetaTitle = gd.MetaTitle,
                    MetaDescription = gd.MetaDescription,
                    MetaKeywords = gd.MetaKeywords,
                    NgayTao = gd.NgayTao,
                    TrangThai = gd.TrangThai,
                    Medias = gd.Media != null ? new List<MediaView>
                    {
                        new MediaView
                        {
                            MaMedia = gd.Media.MaMedia,
                            LoaiMedia = gd.Media.LoaiMedia,
                            DuongDan = gd.Media.DuongDan,
                            AltMedia = gd.Media.AltMedia,
                            LinkMedia = gd.Media.LinkMedia,
                            NgayTao = gd.Media.NgayTao,
                            TrangThai = gd.Media.TrangThai
                        }
                    } : new List<MediaView>()
                }).ToList();
            }, TimeSpan.FromMinutes(30));
        }

        public async Task<GiaoDienView?> GetByIdAsync(int id)
        {
            var cacheKey = $"GiaoDien_GetById_{id}";
            return await _cache.GetOrCreateAsync(cacheKey, async () =>
            {
                var giaoDien = await _context.GiaoDiens
                    .Include(gd => gd.Media)
                    .FirstOrDefaultAsync(gd => gd.MaGiaoDien == id);

                if (giaoDien == null) return null;

                return new GiaoDienView
                {
                    MaGiaoDien = giaoDien.MaGiaoDien,
                    TenGiaoDien = giaoDien.TenGiaoDien,
                    LoaiGiaoDien = giaoDien.LoaiGiaoDien,
                    MoTa = giaoDien.MoTa,
                    MetaTitle = giaoDien.MetaTitle,
                    MetaDescription = giaoDien.MetaDescription,
                    MetaKeywords = giaoDien.MetaKeywords,
                    NgayTao = giaoDien.NgayTao,
                    TrangThai = giaoDien.TrangThai,
                    Medias = giaoDien.Media != null ? new List<MediaView>
                    {
                        new MediaView
                        {
                            MaMedia = giaoDien.Media.MaMedia,
                            LoaiMedia = giaoDien.Media.LoaiMedia,
                            DuongDan = giaoDien.Media.DuongDan,
                            AltMedia = giaoDien.Media.AltMedia,
                            LinkMedia = giaoDien.Media.LinkMedia,
                            NgayTao = giaoDien.Media.NgayTao,
                            TrangThai = giaoDien.Media.TrangThai
                        }
                    } : new List<MediaView>()
                };
            }, TimeSpan.FromMinutes(30));
        }

        public async Task<GiaoDienView?> CreateAsync(Models.Create.GiaoDienCreate giaoDienCreate)
        {
            // Business Logic: Logo chỉ được phép active 1 cái
            if (giaoDienCreate.LoaiGiaoDien == 1 && giaoDienCreate.TrangThai == 1)
            {
                // Disable tất cả Logo khác đang active
                var activeLogos = await _context.GiaoDiens
                    .Where(gd => gd.LoaiGiaoDien == 1 && gd.TrangThai == 1)
                    .ToListAsync();

                foreach (var logo in activeLogos)
                {
                    logo.TrangThai = 0;
                }
            }

            var giaoDien = new GiaoDien
            {
                TenGiaoDien = giaoDienCreate.TenGiaoDien,
                LoaiGiaoDien = giaoDienCreate.LoaiGiaoDien,
                MoTa = giaoDienCreate.MoTa,
                MetaTitle = giaoDienCreate.MetaTitle,
                MetaDescription = giaoDienCreate.MetaDescription,
                MetaKeywords = giaoDienCreate.MetaKeywords,
                TrangThai = giaoDienCreate.TrangThai
            };

            _context.GiaoDiens.Add(giaoDien);
            await _context.SaveChangesAsync();

            // Clear cache
            _cache.Remove("GiaoDien_GetAll");

            return await GetByIdAsync(giaoDien.MaGiaoDien);
        }

        public async Task<GiaoDienView?> UpdateAsync(int id, Models.Edit.GiaoDienEdit giaoDienEdit)
        {
            var giaoDien = await _context.GiaoDiens.FindAsync(id);
            if (giaoDien == null) return null;

            // Business Logic: Logo chỉ được phép active 1 cái
            if (giaoDienEdit.LoaiGiaoDien == 1 && giaoDienEdit.TrangThai == 1)
            {
                // Disable tất cả Logo khác đang active
                var activeLogos = await _context.GiaoDiens
                    .Where(gd => gd.LoaiGiaoDien == 1 && gd.TrangThai == 1 && gd.MaGiaoDien != id)
                    .ToListAsync();

                foreach (var logo in activeLogos)
                {
                    logo.TrangThai = 0;
                }
            }

            // Business Logic: Nếu đang chuyển từ inactive sang active, kiểm tra Logo
            if (giaoDien.TrangThai == 0 && giaoDienEdit.TrangThai == 1 && giaoDienEdit.LoaiGiaoDien == 1)
            {
                // Disable tất cả Logo khác đang active
                var activeLogos = await _context.GiaoDiens
                    .Where(gd => gd.LoaiGiaoDien == 1 && gd.TrangThai == 1 && gd.MaGiaoDien != id)
                    .ToListAsync();

                foreach (var logo in activeLogos)
                {
                    logo.TrangThai = 0;
                }
            }

            giaoDien.TenGiaoDien = giaoDienEdit.TenGiaoDien;
            giaoDien.LoaiGiaoDien = giaoDienEdit.LoaiGiaoDien;
            giaoDien.MoTa = giaoDienEdit.MoTa;
            giaoDien.MetaTitle = giaoDienEdit.MetaTitle;
            giaoDien.MetaDescription = giaoDienEdit.MetaDescription;
            giaoDien.MetaKeywords = giaoDienEdit.MetaKeywords;
            giaoDien.TrangThai = giaoDienEdit.TrangThai;

            await _context.SaveChangesAsync();

            // Clear cache
            var cacheKey = $"GiaoDien_GetById_{id}";
            _cache.Remove(cacheKey);
            _cache.Remove("GiaoDien_GetAll");

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Bắt đầu xóa giao diện: MaGiaoDien={Id}", id);

            var giaoDien = await _context.GiaoDiens
                .Include(gd => gd.Media)
                .FirstOrDefaultAsync(gd => gd.MaGiaoDien == id);

            if (giaoDien == null)
            {
                _logger.LogWarning("Giao diện không tồn tại: MaGiaoDien={Id}", id);
                return false;
            }

            // Xóa file media liên kết nếu có
            if (giaoDien.Media != null && !string.IsNullOrEmpty(giaoDien.Media.DuongDan))
            {
                var mediaDeleted = await _mediaServices.DeleteImageAsync(giaoDien.Media.DuongDan);
                if (mediaDeleted)
                {
                    _logger.LogInformation("Đã xóa file media của giao diện: {MediaUrl}", giaoDien.Media.DuongDan);
                }
                else
                {
                    _logger.LogWarning("Không thể xóa file media của giao diện: {MediaUrl}", giaoDien.Media.DuongDan);
                }
            }

            _context.GiaoDiens.Remove(giaoDien);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Xóa giao diện thành công: MaGiaoDien={Id}", id);

            // Clear cache
            var cacheKey = $"GiaoDien_GetById_{id}";
            _cache.Remove(cacheKey);
            _cache.Remove("GiaoDien_GetAll");

            return true;
        }

        public async Task<IEnumerable<GiaoDienView>> GetByTypeAsync(int loaiGiaoDien)
        {
            return await _context.GiaoDiens
                .Include(gd => gd.Media)
                .Where(gd => gd.LoaiGiaoDien == loaiGiaoDien && gd.TrangThai == 1)
                .OrderBy(gd => gd.NgayTao)
                .Select(gd => new GiaoDienView
                {
                    MaGiaoDien = gd.MaGiaoDien,
                    TenGiaoDien = gd.TenGiaoDien,
                    LoaiGiaoDien = gd.LoaiGiaoDien,
                    MoTa = gd.MoTa,
                    MetaTitle = gd.MetaTitle,
                    MetaDescription = gd.MetaDescription,
                    MetaKeywords = gd.MetaKeywords,
                    NgayTao = gd.NgayTao,
                    TrangThai = gd.TrangThai,
                    Medias = gd.Media != null ? new List<MediaView>
                    {
                        new MediaView
                        {
                            MaMedia = gd.Media.MaMedia,
                            LoaiMedia = gd.Media.LoaiMedia,
                            DuongDan = gd.Media.DuongDan,
                            AltMedia = gd.Media.AltMedia,
                            LinkMedia = gd.Media.LinkMedia,
                            NgayTao = gd.Media.NgayTao,
                            TrangThai = gd.Media.TrangThai
                        }
                    } : new List<MediaView>()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GiaoDienView>> GetActiveAsync()
        {
            return await _context.GiaoDiens
                .Include(gd => gd.Media)
                .Where(gd => gd.TrangThai == 1)
                .OrderBy(gd => gd.LoaiGiaoDien)
                .Select(gd => new GiaoDienView
                {
                    MaGiaoDien = gd.MaGiaoDien,
                    TenGiaoDien = gd.TenGiaoDien,
                    LoaiGiaoDien = gd.LoaiGiaoDien,
                    MoTa = gd.MoTa,
                    MetaTitle = gd.MetaTitle,
                    MetaDescription = gd.MetaDescription,
                    MetaKeywords = gd.MetaKeywords,
                    NgayTao = gd.NgayTao,
                    TrangThai = gd.TrangThai,
                    Medias = gd.Media != null ? new List<MediaView>
                    {
                        new MediaView
                        {
                            MaMedia = gd.Media.MaMedia,
                            LoaiMedia = gd.Media.LoaiMedia,
                            DuongDan = gd.Media.DuongDan,
                            AltMedia = gd.Media.AltMedia,
                            LinkMedia = gd.Media.LinkMedia,
                            NgayTao = gd.Media.NgayTao,
                            TrangThai = gd.Media.TrangThai
                        }
                    } : new List<MediaView>()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GiaoDienView>> SearchAsync(string keyword)
        {
            return await _context.GiaoDiens
                .Include(gd => gd.Media)
                .Where(gd => (gd.TenGiaoDien.Contains(keyword) ||
                             gd.MoTa.Contains(keyword) ||
                             gd.MetaTitle.Contains(keyword)) && gd.TrangThai == 1)
                .OrderBy(gd => gd.NgayTao)
                .Select(gd => new GiaoDienView
                {
                    MaGiaoDien = gd.MaGiaoDien,
                    TenGiaoDien = gd.TenGiaoDien,
                    LoaiGiaoDien = gd.LoaiGiaoDien,
                    MoTa = gd.MoTa,
                    MetaTitle = gd.MetaTitle,
                    MetaDescription = gd.MetaDescription,
                    MetaKeywords = gd.MetaKeywords,
                    NgayTao = gd.NgayTao,
                    TrangThai = gd.TrangThai,
                    Medias = gd.Media != null ? new List<MediaView>
                    {
                        new MediaView
                        {
                            MaMedia = gd.Media.MaMedia,
                            LoaiMedia = gd.Media.LoaiMedia,
                            DuongDan = gd.Media.DuongDan,
                            AltMedia = gd.Media.AltMedia,
                            LinkMedia = gd.Media.LinkMedia,
                            NgayTao = gd.Media.NgayTao,
                            TrangThai = gd.Media.TrangThai
                        }
                    } : new List<MediaView>()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GiaoDienView>> FilterByStatusAsync(int trangThai)
        {
            return await _context.GiaoDiens
                .Include(gd => gd.Media)
                .Where(gd => gd.TrangThai == trangThai)
                .OrderBy(gd => gd.NgayTao)
                .Select(gd => new GiaoDienView
                {
                    MaGiaoDien = gd.MaGiaoDien,
                    TenGiaoDien = gd.TenGiaoDien,
                    LoaiGiaoDien = gd.LoaiGiaoDien,
                    MoTa = gd.MoTa,
                    MetaTitle = gd.MetaTitle,
                    MetaDescription = gd.MetaDescription,
                    MetaKeywords = gd.MetaKeywords,
                    NgayTao = gd.NgayTao,
                    TrangThai = gd.TrangThai,
                    Medias = gd.Media != null ? new List<MediaView>
                    {
                        new MediaView
                        {
                            MaMedia = gd.Media.MaMedia,
                            LoaiMedia = gd.Media.LoaiMedia,
                            DuongDan = gd.Media.DuongDan,
                            AltMedia = gd.Media.AltMedia,
                            LinkMedia = gd.Media.LinkMedia,
                            NgayTao = gd.Media.NgayTao,
                            TrangThai = gd.Media.TrangThai
                        }
                    } : new List<MediaView>()
                })
                .ToListAsync();
        }

        public async Task<bool> AddMediaAsync(int giaoDienId, Models.Create.MediaCreate mediaCreate)
        {
            var giaoDien = await _context.GiaoDiens.FindAsync(giaoDienId);
            if (giaoDien == null) return false;

            var media = new Media
            {
                LoaiMedia = mediaCreate.LoaiMedia,
                DuongDan = mediaCreate.DuongDan,
                AltMedia = mediaCreate.AltMedia,
                LinkMedia = mediaCreate.LinkMedia,
                MaGiaoDien = giaoDienId,
                TrangThai = mediaCreate.TrangThai
            };

            _context.Medias.Add(media);
            await _context.SaveChangesAsync();

            // Clear cache
            var cacheKey = $"GiaoDien_GetById_{giaoDienId}";
            _cache.Remove(cacheKey);
            _cache.Remove("GiaoDien_GetAll");

            return true;
        }

        public async Task<bool> RemoveMediaAsync(int giaoDienId, int mediaId)
        {
            _logger.LogInformation("Bắt đầu xóa media khỏi giao diện: GiaoDienId={GiaoDienId}, MediaId={MediaId}", giaoDienId, mediaId);

            var media = await _context.Medias
                .FirstOrDefaultAsync(m => m.MaMedia == mediaId && m.MaGiaoDien == giaoDienId);

            if (media == null)
            {
                _logger.LogWarning("Không tìm thấy media: GiaoDienId={GiaoDienId}, MediaId={MediaId}", giaoDienId, mediaId);
                return false;
            }

            // Xóa file vật lý nếu có
            if (!string.IsNullOrEmpty(media.DuongDan))
            {
                var fileDeleted = await _mediaServices.DeleteImageAsync(media.DuongDan);
                if (fileDeleted)
                {
                    _logger.LogInformation("Đã xóa file media: {MediaUrl}", media.DuongDan);
                }
                else
                {
                    _logger.LogWarning("Không thể xóa file media: {MediaUrl}", media.DuongDan);
                }
            }

            _context.Medias.Remove(media);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Xóa media khỏi giao diện thành công: GiaoDienId={GiaoDienId}, MediaId={MediaId}", giaoDienId, mediaId);

            // Clear cache
            var cacheKey = $"GiaoDien_GetById_{giaoDienId}";
            _cache.Remove(cacheKey);
            _cache.Remove("GiaoDien_GetAll");

            return true;
        }

        public async Task<IEnumerable<MediaView>> GetMediaAsync(int giaoDienId)
        {
            return await _context.Medias
                .Where(m => m.MaGiaoDien == giaoDienId)
                .Select(m => new MediaView
                {
                    MaMedia = m.MaMedia,
                    LoaiMedia = m.LoaiMedia,
                    DuongDan = m.DuongDan,
                    AltMedia = m.AltMedia,
                    LinkMedia = m.LinkMedia,
                    NgayTao = m.NgayTao,
                    TrangThai = m.TrangThai
                })
                .ToListAsync();
        }
    }
}
