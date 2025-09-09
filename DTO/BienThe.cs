using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionApi.DTO
{
    public class BienThe
    {
        [Key]
        public int MaBienThe { get; set; }

        [StringLength(500)]
        [Url(ErrorMessage = "Hình ảnh phải là URL hợp lệ")]
        public string? HinhAnh { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal GiaBan { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal GiaNhap { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int SoLuongNhap { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int SoLuongBan { get; set; } = 0;

        [Range(0, 100)]
        public decimal KhuyenMai { get; set; } = 0;

        [StringLength(100)]
        public string? MaVach { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [Range(0, 1)]
        public int TrangThai { get; set; } = 1;

        [ForeignKey(nameof(SanPhamNavigation))]
        public int? MaSanPham { get; set; }
        public virtual SanPham? SanPhamNavigation { get; set; }

        [ForeignKey(nameof(DanhMucKichThuoc))]
        public int MaKichThuoc { get; set; }
        public virtual DanhMuc DanhMucKichThuoc { get; set; }

        [ForeignKey(nameof(DanhMucMau))]
        public int MaMau { get; set; }
        public virtual DanhMuc DanhMucMau { get; set; }
    }
}