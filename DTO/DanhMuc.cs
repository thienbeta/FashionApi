using System.ComponentModel.DataAnnotations;

namespace FashionApi.DTO
{
    public class DanhMuc
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Mã danh mục phải lớn hơn 0")]
        public int MaDanhMuc { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Tên danh mục không được vượt quá 50 ký tự")]
        public string TenDanhMuc { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Loại danh mục phải từ 1 đến 5")]
        public int LoaiDanhMuc { get; set; } // 1: Loại, 2: Thương hiệu, 3: Hashtag, 4: Kích thước, 5: Màu sắc

        [StringLength(500, ErrorMessage = "Hình ảnh không được vượt quá 500 ký tự")]
        [Url(ErrorMessage = "Hình ảnh phải là URL hợp lệ")]
        public string? HinhAnh { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [Range(0, 1, ErrorMessage = "Trạng thái phải là 0 hoặc 1")]
        public int TrangThai { get; set; } = 1; // 1: Kích hoạt, 0: Vô hiệu hóa
    }
}