using System.ComponentModel.DataAnnotations;

namespace FashionApi.Models.Create
{
    public class SanPhamCreate
    {
        [Required]
        public string MaSanPham { get; set; }

        [Required]
        public string TenSanPham { get; set; }

        public string? MoTa { get; set; }

        public string? HinhAnh { get; set; }

        [Range(0, double.MaxValue)]
        public decimal GiaBan { get; set; }

        [Range(0, double.MaxValue)]
        public decimal GiaNhap { get; set; }

        [Range(0, int.MaxValue)]
        public int SoLuongNhap { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SoLuongSale { get; set; }

        [Range(0, 100)]
        public double PhanTramSale { get; set; }

        [Required]
        public string Slug { get; set; }

        [Required]
        public string ChatLieu { get; set; }

        [Range(0, 3)]
        public int? GioiTinh { get; set; } = 0; // mặc định là 0

        [Required]
        public int MaKichThuoc { get; set; }

        [Required]
        public int MaMau { get; set; }

        [Required]
        public int MaThuongHieu { get; set; }

        [Required]
        public int MaLoai { get; set; }

        public int? MaHashtag { get; set; }
    }
}
