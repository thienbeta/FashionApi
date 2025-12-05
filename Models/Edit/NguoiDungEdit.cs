namespace FashionApi.Models.Edit
{
    public class NguoiDungEdit
    {
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? Sdt { get; set; }
        public string Email { get; set; }
        public string? TaiKhoan { get; set; }
        public int VaiTro { get; set; } // 0: Khách hàng, 1: Quản trị viên
        public int TrangThai { get; set; } // 1: Kích hoạt, 0: Vô hiệu hóa
        public string? Avt { get; set; }
        public string? TieuSu { get; set; }
        public DateTime? TimeKhoa { get; set; }
        public int? GioiTinh { get; set; } // 0 mặc đinh, 1: Nam, 2: Nữ, 3: khác
        public string? MatKhauCu { get; set; }
        public string? MatKhauMoi { get; set; }
        public string? XacNhanMatKhau { get; set; }
    }
}
