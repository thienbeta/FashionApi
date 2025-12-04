using System.ComponentModel.DataAnnotations;

namespace FashionApi.Models.Edit
{
    public class SanPhamEdit
    {
        public int MaSanPham { get; set; }

        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string? TenSanPham { get; set; }

        [StringLength(10000, ErrorMessage = "Mô tả không được vượt quá 10,000 ký tự")]
        public string? MoTa { get; set; }

        [StringLength(200, ErrorMessage = "Slug không được vượt quá 200 ký tự")]
        public string? Slug { get; set; }

        [StringLength(100, ErrorMessage = "Chất liệu không được vượt quá 100 ký tự")]
        public string? MaVach { get; set; }

        [Range(0, 3, ErrorMessage = "Giới tính phải từ 0 đến 3")]
        public int? GioiTinh { get; set; } // 0: Mặc định, 1: Nam, 2: Nữ, 3: Khác

        public int? MaLoai { get; set; }
        public int? MaThuongHieu { get; set; }
        public int? MaHashtag { get; set; }

        public List<IFormFile>? Images { get; set; }  // Danh sách hình ảnh tải lên

        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        public decimal? GiaBan { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá sale phải lớn hơn hoặc bằng 0")]
        public decimal? GiaSale { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int? SoLuong { get; set; }

        public int? TrangThai { get; set; } // Cập nhật trạng thái
    }
}
