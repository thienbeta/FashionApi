namespace FashionApi.Models.Create
{
    public class MediaCreate
    {
        public string LoaiMedia { get; set; }
        public string DuongDan { get; set; }
        public string? AltMedia { get; set; }
        public string? LinkMedia { get; set; }
        public int? MaSanPham { get; set; }
        public int? MaBinhLuan { get; set; }
    }
}