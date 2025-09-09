using System.ComponentModel.DataAnnotations.Schema;

namespace FashionApi.DTO
{
    public class BinhLuan
    {
        public int MaBinhLuan { get; set; }
        public string? TieuDe { get; set; }
        public string? NoiDung { get; set; }
        public int? DanhGia { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int TrangThai { get; set; }

        public int MaNguoiDung { get; set; }
        public int? MaSanPham { get; set; }

        public virtual NguoiDung NguoiDungNavigation { get; set; }
        public virtual SanPham SanPhamNavigation { get; set; }

        public virtual ICollection<Media> Medias { get; set; } = new List<Media>();

        [NotMapped]
        public IEnumerable<Media> MediaHinhAnh => Medias?.Where(m => m.LoaiMedia == "image");
    }
}
