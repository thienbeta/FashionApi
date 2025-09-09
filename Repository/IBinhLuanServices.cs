using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionApi.Repository
{
    public interface IBinhLuanServices
    {
        Task<BinhLuanView> CreateAsync(BinhLuanCreate model);
        Task<BinhLuanView> UpdateAsync(int id, BinhLuanEdit model, List<IFormFile>? newImageFiles = null);
        Task<bool> DeleteAsync(int id);
        Task<BinhLuanView> GetByIdAsync(int id);
        Task<List<BinhLuanView>> GetAllAsync();
        Task<List<BinhLuanView>> SearchAsync(int? danhGia, int? trangThai, int? maSanPham, int? maNguoiDung);
        Task<List<BinhLuanView>> FilterByTrangThaiAsync(int trangThai);
        Task<List<BinhLuanView>> FilterByDanhGiaAsync(int danhGia);
    }
}