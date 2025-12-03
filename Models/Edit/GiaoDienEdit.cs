using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FashionApi.Models.Edit
{
    public class GiaoDienEdit
    {
        [Required(ErrorMessage = "Mã giao diện là bắt buộc")]
        public int MaGiaoDien { get; set; }

        [Required(ErrorMessage = "Tên giao diện là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên giao diện không được vượt quá 100 ký tự")]
        public string TenGiaoDien { get; set; }

        [Required(ErrorMessage = "Loại giao diện là bắt buộc")]
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

        public int TrangThai { get; set; } = 1;

        // Hình ảnh upload mới
        public List<IFormFile>? HinhAnhs { get; set; }

        // Danh sách ID media cần xóa
        public List<int>? XoaMediaIds { get; set; }
    }
}