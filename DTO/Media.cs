using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionApi.DTO
{
    public class Media
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Mã media phải lớn hơn 0")]
        public int MaMedia { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Loại media không được vượt quá 50 ký tự")]
        public string LoaiMedia { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Đường dẫn không được vượt quá 500 ký tự")]
        [Url(ErrorMessage = "Đường dẫn phải là URL hợp lệ")]
        public string DuongDan { get; set; }

        [StringLength(200, ErrorMessage = "Alt media không được vượt quá 200 ký tự")]
        public string? AltMedia { get; set; }

        [StringLength(500, ErrorMessage = "Link media không được vượt quá 500 ký tự")]
        [Url(ErrorMessage = "Link media phải là URL hợp lệ")]
        public string? LinkMedia { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [Range(0, 1, ErrorMessage = "Trạng thái phải là 0 hoặc 1")]
        public int TrangThai { get; set; } = 1; // 1: Kích hoạt, 0: Vô hiệu hóa

        // Quan hệ
        public int? MaSanPham { get; set; }
        [ForeignKey("MaSanPham")]
        public virtual SanPham? SanPhamNavigation { get; set; }

        public int? MaBinhLuan { get; set; }
        [ForeignKey("MaBinhLuan")]
        public virtual BinhLuan? BinhLuanNavigation { get; set; }
    }
}