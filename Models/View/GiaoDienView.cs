using FashionApi.DTO;

namespace FashionApi.Models.View
{
    public class GiaoDienView
    {
        public int MaGiaoDien { get; set; }
        public string TenGiaoDien { get; set; }
        public int LoaiGiaoDien { get; set; }
        public string? MoTa { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public DateTime NgayTao { get; set; }
        public int TrangThai { get; set; }
        public List<MediaView>? Medias { get; set; }
    }
}