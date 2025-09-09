using System.ComponentModel.DataAnnotations;

namespace FashionApi.Models.Create
{
    public class BienTheCreate
    {
        public string? HinhAnh { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal GiaBan { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal GiaNhap { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int SoLuongNhap { get; set; }

        public int SoLuongBan { get; set; } = 0;

        [Range(0, 100)]
        public decimal KhuyenMai { get; set; } = 0;

        public string? MaVach { get; set; }

        [Required]
        public int MaSanPham { get; set; }

        [Required]
        public int MaKichThuoc { get; set; }

        [Required]
        public int MaMau { get; set; }
    }
}