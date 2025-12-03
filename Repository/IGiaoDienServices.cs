using FashionApi.DTO;
using FashionApi.Models.View;

namespace FashionApi.Repository
{
    public interface IGiaoDienServices
    {
        // CRUD operations
        Task<IEnumerable<GiaoDienView>> GetAllAsync();
        Task<GiaoDienView?> GetByIdAsync(int id);
        Task<GiaoDienView?> CreateAsync(Models.Create.GiaoDienCreate giaoDienCreate);
        Task<GiaoDienView?> UpdateAsync(int id, Models.Edit.GiaoDienEdit giaoDienEdit);
        Task<bool> DeleteAsync(int id);

        // Filtering and search
        Task<IEnumerable<GiaoDienView>> GetByTypeAsync(int loaiGiaoDien);
        Task<IEnumerable<GiaoDienView>> GetActiveAsync();
        Task<IEnumerable<GiaoDienView>> SearchAsync(string keyword);
        Task<IEnumerable<GiaoDienView>> FilterByStatusAsync(int trangThai);

        // Media operations
        Task<bool> AddMediaAsync(int giaoDienId, Models.Create.MediaCreate mediaCreate);
        Task<bool> RemoveMediaAsync(int giaoDienId, int mediaId);
        Task<IEnumerable<MediaView>> GetMediaAsync(int giaoDienId);
    }
}