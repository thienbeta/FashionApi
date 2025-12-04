using System.ComponentModel.DataAnnotations;

namespace FashionApi.Models.Create
{
    public class NguoiDungCreate
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tài khoản là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tài khoản không được vượt quá 50 ký tự")]
        public string TaiKhoan { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8-100 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,100}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ thường, 1 chữ hoa, 1 số và 1 ký tự đặc biệt")]
        public string? MatKhau { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [Range(1, 2, ErrorMessage = "Vai trò phải là 1 (Admin) hoặc 2 (User)")]
        public int VaiTro { get; set; }
    }
}
