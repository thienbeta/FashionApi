namespace FashionApi.Models.View
{
    public class LoaiView
    {
        public int MaLoai { get; set; }
        public string TenLoai { get; set; }
        public string? MoTa { get; set; }
        public string KiHieu { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int TrangThai { get; set; } // 1: Kích hoạt, 0: Vô hiệu hóa
    }
}
