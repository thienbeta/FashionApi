namespace FashionApi.DTO
{
    public class ThuongHieu
    {
        public int MaThuongHieu { get; set; }
        public string TenThuongHieu { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int TrangThai { get; set; } = 1; // 1: Kích hoạt, 0: Vô hiệu hóa

        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}
