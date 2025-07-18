using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.DTO;
using Microsoft.AspNetCore.Http;

namespace FashionApi.Repository
{
    public interface IThuongHieuServices
    {
        Task<List<ThuongHieuView>> GetAllAsync();
        Task<ThuongHieuView?> GetByIdAsync(int id);
        Task<List<ThuongHieuView>> SearchAsync(string keyword);
        Task<List<SanPham>> GetSanPhamsByThuongHieuIdAsync(int id);
        Task<bool> CreateAsync(ThuongHieuCreate model, IFormFile? file);
        Task<bool> UpdateAsync(ThuongHieuEdit model, IFormFile? file);
        Task<bool> DeleteAsync(int id);
    }
}
