namespace FashionApi.DTO
{
    public class Hashtag
    {
        public int MaHashtag { get; set; }
        public string TenHashtag { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int TrangThai { get; set; } = 1; // 1: Kích hoạt, 0: Vô hiệu hóa

        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}
