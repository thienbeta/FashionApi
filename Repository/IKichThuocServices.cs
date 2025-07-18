using FashionApi.DTO;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using Microsoft.AspNetCore.Http;

namespace FashionApi.Repository
{
    public interface IKichThuocServices
    {
        Task<List<KichThuocView>> GetAllAsync();
        Task<KichThuocView?> GetByIdAsync(int id);
        Task<List<KichThuocView>> SearchAsync(string keyword);
        Task<List<SanPham>> GetSanPhamsByKichThuocIdAsync(int id);
        Task<bool> CreateAsync(KichThuocCreate model, IFormFile? file);
        Task<bool> UpdateAsync(KichThuocEdit model, IFormFile? file);
        Task<bool> DeleteAsync(int id);
    }
}