using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionApi.Repository
{
    public interface ISanPhamServices
    {
        Task<SanPhamView> CreateAsync(SanPhamCreate model);
        Task<SanPhamView> UpdateAsync(int id, SanPhamEdit model, List<IFormFile>? newImageFiles = null);
        Task<bool> DeleteAsync(int id);
        Task<SanPhamView> GetByIdAsync(int id);
        Task<List<SanPhamView>> GetAllAsync();
        Task<List<SanPhamView>> SearchAsync(decimal? giaBan, int? soLuongNhap, int? trangThai, string? maVach, int? maSanPham, string? tenSanPham);
        Task<List<SanPhamView>> FilterByLoaiDanhMucAsync(int maLoaiDanhMuc);
        Task<List<SanPhamView>> GetByDanhMucAsync(int maDanhMuc);
        Task<List<SanPhamView>> GetBestSellingAsync(int limit);
        Task<List<SanPhamView>> GetByLoaiAndThuongHieuAsync(int maLoai, int maThuongHieu);
    }
}