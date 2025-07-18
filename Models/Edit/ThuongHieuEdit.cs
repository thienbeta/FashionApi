namespace FashionApi.Models.Edit
{
    public class ThuongHieuEdit
    {
        public int MaThuongHieu { get; set; }
        public string TenThuongHieu { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public int TrangThai { get; set; } // 1: Kích hoạt, 0: Vô hiệu hóa
    }
}
