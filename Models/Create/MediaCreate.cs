namespace FashionApi.Models.Create
{
    public class MediaCreate
    {
        public string LoaiMedia { get; set; } = null!;
        public string DuongDan { get; set; } = null!;
        public string? AltMedia { get; set; }
        public string? LinkMedia { get; set; }
        public int? MaSanPham { get; set; }
        public int? MaBinhLuan { get; set; }
        public int? MaGiaoDien { get; set; }
        public int TrangThai { get; set; } = 1;
    }
}