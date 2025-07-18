namespace FashionApi.Models.View
{
    public class KichThuocView
    {
        public int MaKichThuoc { get; set; }
        public string TenKichThuoc { get; set; } = null!;
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int TrangThai { get; set; } // 1: Kích hoạt, 0: Vô hiệu hóa
    }
}
