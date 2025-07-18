using FashionApi.DTO;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using Microsoft.AspNetCore.Http;

namespace FashionApi.Repository
{
    public interface ILoaiServices
    {
        Task<List<LoaiView>> GetAllAsync();
        Task<LoaiView?> GetByIdAsync(int id);
        Task<List<LoaiView>> SearchAsync(string keyword);
        Task<List<SanPham>> GetSanPhamsByLoaiIdAsync(int id);
        Task<bool> CreateAsync(LoaiCreate model, IFormFile? file);
        Task<bool> UpdateAsync(LoaiEdit model, IFormFile? file);
        Task<bool> DeleteAsync(int id);
    }
}
