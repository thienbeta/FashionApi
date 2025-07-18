namespace FashionApi.Models.View
{
    public class MediaView
    {
        public int MaMedia { get; set; }
        public string LoaiMedia { get; set; }
        public string? DuongDan { get; set; }
        public string? AltMedia { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int TrangThai { get; set; } = 1; // 1: Kích hoạt, 0: Vô hiệu hóa
        public string? MaSanPham { get; set; }

    }
}
