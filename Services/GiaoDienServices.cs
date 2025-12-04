using FashionApi.DTO;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.EntityFrameworkCore;

namespace FashionApi.Services
{
    public class GiaoDienServices : IGiaoDienServices
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly IMemoryCacheServices _cache;

        public GiaoDienServices(Data.ApplicationDbContext context, IMemoryCacheServices cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<GiaoDienView>> GetAllAsync()
        {
            return await _cache.GetOrCreateAsync("GiaoDien_GetAll", async () =>
            {
                var giaoDiens = await _context.GiaoDiens
                    .Include(gd => gd.Medias)
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
                    Medias = gd.Medias.Select(m => new MediaView
                    {
                        MaMedia = m.MaMedia,
                        LoaiMedia = m.LoaiMedia,
                        DuongDan = m.DuongDan,
                        AltMedia = m.AltMedia,
                        LinkMedia = m.LinkMedia,
                        NgayTao = m.NgayTao,
                        TrangThai = m.TrangThai
                    }).ToList()
                }).ToList();
            }, TimeSpan.FromMinutes(30));
        }

        public async Task<GiaoDienView?> GetByIdAsync(int id)
        {
            var cacheKey = $"GiaoDien_GetById_{id}";
            return await _cache.GetOrCreateAsync(cacheKey, async () =>
            {
                var giaoDien = await _context.GiaoDiens
                    .Include(gd => gd.Medias)
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
                    Medias = giaoDien.Medias.Select(m => new MediaView
                    {
                        MaMedia = m.MaMedia,
                        LoaiMedia = m.LoaiMedia,
                        DuongDan = m.DuongDan,
                        AltMedia = m.AltMedia,
                        LinkMedia = m.LinkMedia,
                        NgayTao = m.NgayTao,
                        TrangThai = m.TrangThai
                    }).ToList()
                };
            }, TimeSpan.FromMinutes(30));
        }

        public async Task<GiaoDienView?> CreateAsync(Models.Create.GiaoDienCreate giaoDienCreate)
        {
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
            var giaoDien = await _context.GiaoDiens
                .Include(gd => gd.Medias)
                .FirstOrDefaultAsync(gd => gd.MaGiaoDien == id);

            if (giaoDien == null) return false;

            _context.GiaoDiens.Remove(giaoDien);
            await _context.SaveChangesAsync();

            // Clear cache
            var cacheKey = $"GiaoDien_GetById_{id}";
            _cache.Remove(cacheKey);
            _cache.Remove("GiaoDien_GetAll");

            return true;
        }

        public async Task<IEnumerable<GiaoDienView>> GetByTypeAsync(int loaiGiaoDien)
        {
            return await _context.GiaoDiens
                .Include(gd => gd.Medias)
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
                    Medias = gd.Medias.Select(m => new MediaView
                    {
                        MaMedia = m.MaMedia,
                        LoaiMedia = m.LoaiMedia,
                        DuongDan = m.DuongDan,
                        AltMedia = m.AltMedia,
                        LinkMedia = m.LinkMedia,
                        NgayTao = m.NgayTao,
                        TrangThai = m.TrangThai
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GiaoDienView>> GetActiveAsync()
        {
            return await _context.GiaoDiens
                .Include(gd => gd.Medias)
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
                    Medias = gd.Medias.Select(m => new MediaView
                    {
                        MaMedia = m.MaMedia,
                        LoaiMedia = m.LoaiMedia,
                        DuongDan = m.DuongDan,
                        AltMedia = m.AltMedia,
                        LinkMedia = m.LinkMedia,
                        NgayTao = m.NgayTao,
                        TrangThai = m.TrangThai
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GiaoDienView>> SearchAsync(string keyword)
        {
            return await _context.GiaoDiens
                .Include(gd => gd.Medias)
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
                    Medias = gd.Medias.Select(m => new MediaView
                    {
                        MaMedia = m.MaMedia,
                        LoaiMedia = m.LoaiMedia,
                        DuongDan = m.DuongDan,
                        AltMedia = m.AltMedia,
                        LinkMedia = m.LinkMedia,
                        NgayTao = m.NgayTao,
                        TrangThai = m.TrangThai
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GiaoDienView>> FilterByStatusAsync(int trangThai)
        {
            return await _context.GiaoDiens
                .Include(gd => gd.Medias)
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
                    Medias = gd.Medias.Select(m => new MediaView
                    {
                        MaMedia = m.MaMedia,
                        LoaiMedia = m.LoaiMedia,
                        DuongDan = m.DuongDan,
                        AltMedia = m.AltMedia,
                        LinkMedia = m.LinkMedia,
                        NgayTao = m.NgayTao,
                        TrangThai = m.TrangThai
                    }).ToList()
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
            var media = await _context.Medias
                .FirstOrDefaultAsync(m => m.MaMedia == mediaId && m.MaGiaoDien == giaoDienId);

            if (media == null) return false;

            _context.Medias.Remove(media);
            await _context.SaveChangesAsync();

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