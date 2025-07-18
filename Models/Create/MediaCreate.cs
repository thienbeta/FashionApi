namespace FashionApi.Models.Create
{
    public class MediaCreate
    {
        public string LoaiMedia { get; set; }
        public string? DuongDan { get; set; }
        public string? AltMedia { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int TrangThai { get; set; }
        public string? MaSanPham { get; set; }
    }
}
