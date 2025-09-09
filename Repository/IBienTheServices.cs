using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionApi.Repository
{
    public interface IBienTheServices
    {
        Task<BienTheView> CreateAsync(BienTheCreate bienTheCreate, IFormFile imageFile);
        Task<BienTheView> UpdateAsync(int id, BienTheEdit bienTheEdit, IFormFile imageFile = null);
        Task<bool> DeleteAsync(int id);
        Task<BienTheView> GetByIdAsync(int id);
        Task<List<BienTheView>> GetAllAsync();
        Task<List<BienTheView>> SearchAsync(decimal? giaBan, int? soLuongNhap, int? trangThai);
        Task<List<BienTheView>> FilterByDanhMucAsync(int maDanhMuc);
        Task<List<BienTheView>> GetBySanPhamAsync(int maSanPham);
    }
}