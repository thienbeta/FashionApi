using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionApi.Repository
{
    public interface IDanhMucServices
    {
        Task<DanhMucView> CreateAsync(DanhMucCreate model, IFormFile imageFile);
        Task<DanhMucView> UpdateAsync(int id, DanhMucEdit model, IFormFile imageFile = null);
        Task<bool> DeleteAsync(int id);
        Task<DanhMucView> GetByIdAsync(int id);
        Task<IEnumerable<DanhMucView>> GetAllAsync();
        Task<IEnumerable<DanhMucView>> SearchAsync(string keyword);
        Task<IEnumerable<DanhMucView>> FilterByStatusAsync(int status);
        Task<IEnumerable<DanhMucView>> FilterByCategoryTypeAsync(int loaiDanhMuc);
    }
}