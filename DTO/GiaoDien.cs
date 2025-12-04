using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionApi.DTO
{
    public class GiaoDien
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Mã giao diện phải lớn hơn 0")]
        public int MaGiaoDien { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Tên giao diện không được vượt quá 100 ký tự")]
        public string TenGiaoDien { get; set; } = string.Empty;

        [Required]
        [Range(1, 3, ErrorMessage = "Loại giao diện phải từ 1 đến 3")]
        public int LoaiGiaoDien { get; set; } // 1: Logo, 2: Banner, 3: Slider

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? MoTa { get; set; }

        // SEO fields
        [StringLength(200, ErrorMessage = "Meta title không được vượt quá 200 ký tự")]
        public string? MetaTitle { get; set; }

        [StringLength(500, ErrorMessage = "Meta description không được vượt quá 500 ký tự")]
        public string? MetaDescription { get; set; }

        [StringLength(500, ErrorMessage = "Meta keywords không được vượt quá 500 ký tự")]
        public string? MetaKeywords { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [Range(0, 1, ErrorMessage = "Trạng thái phải là 0 hoặc 1")]
        public int TrangThai { get; set; } = 1; // 1: Kích hoạt, 0: Vô hiệu hóa

        // Quan hệ 1-1 với Media
        public virtual Media? Media { get; set; }
    }
}
