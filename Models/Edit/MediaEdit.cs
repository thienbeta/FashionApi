namespace FashionApi.Models.Edit
{
    public class MediaEdit
    {
        public int MaMedia { get; set; }

        public string LoaiMedia { get; set; }

        public string? DuongDan { get; set; }

        public string? AltMedia { get; set; }

        public int TrangThai { get; set; } = 1;

        public string? MaSanPham { get; set; }
    }
}
