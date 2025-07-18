using FashionApi.DTO;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using Microsoft.AspNetCore.Http;

namespace FashionApi.Repository
{
    public interface IMauServices
    {
        Task<List<MauView>> GetAllAsync();
        Task<MauView?> GetByIdAsync(int id);
        Task<List<MauView>> SearchAsync(string keyword);
        Task<List<SanPham>> GetSanPhamsByMauIdAsync(int id);
        Task<bool> CreateAsync(MauCreate model, IFormFile? file);
        Task<bool> UpdateAsync(MauEdit model, IFormFile? file);
        Task<bool> DeleteAsync(int id);
    }
}
