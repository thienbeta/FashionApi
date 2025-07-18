using FashionApi.DTO;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using Microsoft.AspNetCore.Http;

namespace FashionApi.Repository
{
    public interface IHashtagServices
    {
        Task<List<HashtagView>> GetAllAsync();
        Task<HashtagView?> GetByIdAsync(int id);
        Task<List<HashtagView>> SearchAsync(string keyword);
        Task<List<SanPham>> GetSanPhamsByHashtagIdAsync(int id);
        Task<bool> CreateAsync(HashtagCreate model, IFormFile? file);
        Task<bool> UpdateAsync(HashtagEdit model, IFormFile? file);
        Task<bool> DeleteAsync(int id);
    }
}
