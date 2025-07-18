// File: FashionApi/Services/SanPhamServices.cs
using FashionApi.Data;
using FashionApi.DTO;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.EntityFrameworkCore;

namespace FashionApi.Services
{
    public class SanPhamServices : ISanPhamServices
    {
        private readonly ApplicationDbContext _context;

        public SanPhamServices(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> CreateAsync(SanPhamCreate model)
        {
            // 1. Kiểm tra loại
            var loai = await _context.Loais.FindAsync(model.MaLoai);
            if (loai == null)
                return false;

            var prefix = loai.KiHieu.Trim().ToUpper();

            // 2. Lấy mã gốc có prefix (ví dụ AT)
            var baseCodes = await _context.SanPhams
                .Where(sp => sp.MaSanPham.StartsWith(prefix + "_"))
                .Select(sp => sp.MaSanPham)
                .ToListAsync();

            // 3. Tìm mã gốc lớn nhất hiện tại (ví dụ AT_1, AT_2)
            int maxBase = baseCodes
                .Select(code => code.Split('_'))
                .Where(parts => parts.Length >= 2 && int.TryParse(parts[1], out _))
                .Select(parts => int.Parse(parts[1]))
                .DefaultIfEmpty(0)
                .Max();

            // 4. Tìm mã gốc hiện tại (ví dụ AT_1)
            var baseCode = $"{prefix}_{maxBase}";

            // 5. Kiểm tra nếu mã gốc chưa có dòng nào, thì là dòng đầu tiên → AT_1
            bool baseCodeExists = baseCodes.Any(code => code == baseCode);

            string finalCode;
            if (!baseCodeExists)
            {
                finalCode = baseCode; // Sản phẩm đầu tiên của loại
            }
            else
            {
                // Lấy các mã biến thể AT_1_1, AT_1_2...
                var variants = baseCodes
                    .Where(code => code.StartsWith(baseCode + "_"))
                    .Select(code => code.Substring(baseCode.Length + 1))
                    .Select(suffix => int.TryParse(suffix, out var n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .ToList();

                int nextVariant = variants.Any() ? variants.Max() + 1 : 1;
                finalCode = $"{baseCode}_{nextVariant}";
            }

            // 6. Kiểm tra trùng slug
            bool slugExists = await _context.SanPhams.AnyAsync(sp => sp.Slug == model.Slug.Trim());
            if (slugExists)
                return false;

            // 7. Tạo entity
            var entity = new SanPham
            {
                MaSanPham = finalCode,
                TenSanPham = model.TenSanPham.Trim(),
                MoTa = model.MoTa,
                HinhAnh = model.HinhAnh,
                GiaBan = model.GiaBan,
                GiaNhap = model.GiaNhap,
                SoLuongNhap = model.SoLuongNhap,
                SoLuongBan = 0,
                SoLuongSale = model.SoLuongSale,
                PhanTramSale = model.PhanTramSale,
                Slug = model.Slug.Trim(),
                ChatLieu = model.ChatLieu.Trim(),
                GioiTinh = model.GioiTinh ?? 0,
                MaKichThuoc = model.MaKichThuoc,
                MaMau = model.MaMau,
                MaThuongHieu = model.MaThuongHieu,
                MaLoai = model.MaLoai,
                MaHashtag = model.MaHashtag,
                TrangThai = 1,
                NgayTao = DateTime.UtcNow
            };

            // 8. Lưu DB
            _context.SanPhams.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(SanPhamEdit model)
        {
            var sp = await _context.SanPhams.FindAsync(model.MaSanPham);
            if (sp == null) return false;

            // Cập nhật thuộc tính
            sp.TenSanPham = model.TenSanPham.Trim();
            sp.MoTa = model.MoTa;
            sp.HinhAnh = model.HinhAnh;
            sp.GiaBan = model.GiaBan;
            sp.GiaNhap = model.GiaNhap;
            sp.SoLuongNhap = model.SoLuongNhap;
            sp.SoLuongBan = model.SoLuongBan;
            sp.SoLuongSale = model.SoLuongSale;
            sp.PhanTramSale = model.PhanTramSale;
            sp.Slug = model.Slug.Trim();
            sp.ChatLieu = model.ChatLieu.Trim();
            sp.GioiTinh = model.GioiTinh ?? sp.GioiTinh;
            sp.MaKichThuoc = model.MaKichThuoc;
            sp.MaMau = model.MaMau;
            sp.MaThuongHieu = model.MaThuongHieu;
            sp.MaLoai = model.MaLoai;
            sp.MaHashtag = model.MaHashtag;
            sp.TrangThai = model.TrangThai;

            _context.SanPhams.Update(sp);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string maSanPham)
        {
            var sp = await _context.SanPhams.FindAsync(maSanPham);
            if (sp == null) return false;

            _context.SanPhams.Remove(sp);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SanPhamView?> GetByIdAsync(string maSanPham)
        {
            return await _context.SanPhams
                .Include(sp => sp.KichThuocNavigation)
                .Include(sp => sp.MauNavigation)
                .Include(sp => sp.ThuongHieuNavigation)
                .Include(sp => sp.LoaiNavigation)
                .Include(sp => sp.HashtagNavigation)
                .Include(sp => sp.Medias)
                .Where(sp => sp.MaSanPham == maSanPham)
                .Select(sp => new SanPhamView
                {
                    MaSanPham = sp.MaSanPham,
                    TenSanPham = sp.TenSanPham,
                    MoTa = sp.MoTa,
                    HinhAnh = sp.HinhAnh,
                    GiaBan = sp.GiaBan,
                    GiaNhap = sp.GiaNhap,
                    SoLuongNhap = sp.SoLuongNhap,
                    SoLuongBan = sp.SoLuongBan,
                    SoLuongSale = sp.SoLuongSale,
                    PhanTramSale = sp.PhanTramSale,
                    Slug = sp.Slug,
                    ChatLieu = sp.ChatLieu,
                    GioiTinh = sp.GioiTinh,
                    NgayTao = sp.NgayTao,
                    TrangThai = sp.TrangThai,
                    MaKichThuoc = sp.MaKichThuoc,
                    MaMau = sp.MaMau,
                    MaThuongHieu = sp.MaThuongHieu,
                    MaLoai = sp.MaLoai,
                    MaHashtag = sp.MaHashtag,
                    TenKichThuoc = sp.KichThuocNavigation.TenKichThuoc,
                    TenMau = sp.MauNavigation.TenMau,
                    TenThuongHieu = sp.ThuongHieuNavigation.TenThuongHieu,
                    TenLoai = sp.LoaiNavigation.TenLoai,
                    TenHashtag = sp.HashtagNavigation!.TenHashtag,
                    Medias = sp.Medias.Select(m => new MediaView
                    {
                        MaMedia = m.MaMedia,
                        LoaiMedia = m.LoaiMedia,
                        DuongDan = m.DuongDan,
                        AltMedia = m.AltMedia,
                        NgayTao = m.NgayTao,
                        TrangThai = m.TrangThai,
                        MaSanPham = m.MaSanPham
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<SanPhamView>> GetAllAsync()
        {
            return await _context.SanPhams
                .Include(sp => sp.KichThuocNavigation)
                .Include(sp => sp.MauNavigation)
                .Include(sp => sp.ThuongHieuNavigation)
                .Include(sp => sp.LoaiNavigation)
                .Include(sp => sp.HashtagNavigation)
                .Include(sp => sp.Medias)
                .Select(sp => new SanPhamView
                {
                    MaSanPham = sp.MaSanPham,
                    TenSanPham = sp.TenSanPham,
                    MoTa = sp.MoTa,
                    HinhAnh = sp.HinhAnh,
                    GiaBan = sp.GiaBan,
                    GiaNhap = sp.GiaNhap,
                    SoLuongNhap = sp.SoLuongNhap,
                    SoLuongBan = sp.SoLuongBan,
                    SoLuongSale = sp.SoLuongSale,
                    PhanTramSale = sp.PhanTramSale,
                    Slug = sp.Slug,
                    ChatLieu = sp.ChatLieu,
                    GioiTinh = sp.GioiTinh,
                    NgayTao = sp.NgayTao,
                    TrangThai = sp.TrangThai,
                    MaKichThuoc = sp.MaKichThuoc,
                    MaMau = sp.MaMau,
                    MaThuongHieu = sp.MaThuongHieu,
                    MaLoai = sp.MaLoai,
                    MaHashtag = sp.MaHashtag,
                    TenKichThuoc = sp.KichThuocNavigation.TenKichThuoc,
                    TenMau = sp.MauNavigation.TenMau,
                    TenThuongHieu = sp.ThuongHieuNavigation.TenThuongHieu,
                    TenLoai = sp.LoaiNavigation.TenLoai,
                    TenHashtag = sp.HashtagNavigation!.TenHashtag,
                    Medias = sp.Medias.Select(m => new MediaView
                    {
                        MaMedia = m.MaMedia,
                        LoaiMedia = m.LoaiMedia,
                        DuongDan = m.DuongDan,
                        AltMedia = m.AltMedia,
                        NgayTao = m.NgayTao,
                        TrangThai = m.TrangThai,
                        MaSanPham = m.MaSanPham
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<SanPhamView>> SearchAsync(string keyword)
        {
            return await _context.SanPhams
                .Include(sp => sp.KichThuocNavigation)
                .Include(sp => sp.MauNavigation)
                .Include(sp => sp.ThuongHieuNavigation)
                .Include(sp => sp.LoaiNavigation)
                .Include(sp => sp.HashtagNavigation)
                .Include(sp => sp.Medias)
                .Where(sp =>
                    sp.TenSanPham.Contains(keyword) ||
                    sp.Slug.Contains(keyword) ||
                    sp.MaSanPham.Contains(keyword))
                .Select(sp => new SanPhamView
                {
                    // giống projection của GetAllAsync
                    MaSanPham = sp.MaSanPham,
                    TenSanPham = sp.TenSanPham,
                    MoTa = sp.MoTa,
                    HinhAnh = sp.HinhAnh,
                    GiaBan = sp.GiaBan,
                    GiaNhap = sp.GiaNhap,
                    SoLuongNhap = sp.SoLuongNhap,
                    SoLuongBan = sp.SoLuongBan,
                    SoLuongSale = sp.SoLuongSale,
                    PhanTramSale = sp.PhanTramSale,
                    Slug = sp.Slug,
                    ChatLieu = sp.ChatLieu,
                    GioiTinh = sp.GioiTinh,
                    NgayTao = sp.NgayTao,
                    TrangThai = sp.TrangThai,
                    MaKichThuoc = sp.MaKichThuoc,
                    MaMau = sp.MaMau,
                    MaThuongHieu = sp.MaThuongHieu,
                    MaLoai = sp.MaLoai,
                    MaHashtag = sp.MaHashtag,
                    TenKichThuoc = sp.KichThuocNavigation.TenKichThuoc,
                    TenMau = sp.MauNavigation.TenMau,
                    TenThuongHieu = sp.ThuongHieuNavigation.TenThuongHieu,
                    TenLoai = sp.LoaiNavigation.TenLoai,
                    TenHashtag = sp.HashtagNavigation!.TenHashtag,
                    Medias = sp.Medias.Select(m => new MediaView
                    {
                        MaMedia = m.MaMedia,
                        LoaiMedia = m.LoaiMedia,
                        DuongDan = m.DuongDan,
                        AltMedia = m.AltMedia,
                        NgayTao = m.NgayTao,
                        TrangThai = m.TrangThai,
                        MaSanPham = m.MaSanPham
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}
