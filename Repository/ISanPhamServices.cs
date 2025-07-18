using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;

namespace FashionApi.Repository
{
    public interface ISanPhamServices
    {
        Task<bool> CreateAsync(SanPhamCreate model);
        Task<bool> UpdateAsync(SanPhamEdit model);
        Task<bool> DeleteAsync(string maSanPham);
        Task<SanPhamView?> GetByIdAsync(string maSanPham);
        Task<List<SanPhamView>> GetAllAsync();
        Task<List<SanPhamView>> SearchAsync(string keyword);
    }
}
