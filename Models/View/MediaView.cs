namespace FashionApi.Models.View
{
    public class MediaView
    {
        public int MaMedia { get; set; }
        public string LoaiMedia { get; set; }
        public string DuongDan { get; set; }
        public string? AltMedia { get; set; }
        public string? LinkMedia { get; set; }
        public DateTime NgayTao { get; set; }
        public int TrangThai { get; set; }
        public int? MaSanPham { get; set; }
        public string? TenSanPham { get; set; } // Tên sản phẩm để hiển thị
        public int? MaBinhLuan { get; set; }
    }
}