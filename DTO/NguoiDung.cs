using System.ComponentModel.DataAnnotations;

namespace FashionApi.DTO
{
    public class NguoiDung
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Mã người dùng phải lớn hơn 0")]
        public int MaNguoiDung { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; }

        public DateTime? NgaySinh { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? Sdt { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Tài khoản không được vượt quá 50 ký tự")]
        public string TaiKhoan { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Mật khẩu không được vượt quá 100 ký tự")]
        public string MatKhau { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Vai trò phải lớn hơn hoặc bằng 0")]
        public int VaiTro { get; set; } = 0;

        [Range(0, 1, ErrorMessage = "Trạng thái phải là 0 hoặc 1")]
        public int TrangThai { get; set; } = 1;

        [StringLength(500, ErrorMessage = "Ảnh đại diện không được vượt quá 500 ký tự")]
        [Url(ErrorMessage = "Ảnh đại diện phải là URL hợp lệ")]
        public string? Avt { get; set; }

        [StringLength(1000, ErrorMessage = "Tiểu sử không được vượt quá 1000 ký tự")]
        public string? TieuSu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        public DateTime? TimeKhoa { get; set; }

        [Range(0, 3, ErrorMessage = "Giới tính phải từ 0 đến 3")]
        public int GioiTinh { get; set; } = 0;

        public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();
    }
}