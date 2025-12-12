using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;

namespace FashionApi.Repository
{
    public interface INguoiDungServices
    {
        Task<NguoiDungView> CreateAsync(NguoiDungCreate model);
        Task<NguoiDungView> UpdateAsync(int id, NguoiDungEdit model, IFormFile imageFile = null, int? currentUserId = null);
        Task<bool> DeleteAsync(int id);
        Task<NguoiDungView> GetByIdAsync(int id);
        Task<List<NguoiDungView>> GetAllAsync();
        Task<List<NguoiDungView>> SearchAsync(string keyword);
        Task<List<NguoiDungView>> FilterByRoleAsync(int role);
        Task<LoginResponse> LoginAsync(string taiKhoan, string matKhau);
        Task SendForgotPasswordOtpAsync(string email);
        Task ResetPasswordAsync(QuenMatKhau model);
        Task<bool> CheckAccountExistsAsync(string taiKhoan);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> CheckPhoneExistsAsync(string phone);
    }
}
