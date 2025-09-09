using System.ComponentModel.DataAnnotations;

namespace FashionApi.Models.Edit
{
    public class BienTheEdit
    {
        [Required]
        public int MaBienThe { get; set; }

        public string? HinhAnh { get; set; }
        public decimal? GiaBan { get; set; }
        public decimal? GiaNhap { get; set; }
        public int? SoLuongNhap { get; set; }
        public int? SoLuongBan { get; set; }
        public decimal? KhuyenMai { get; set; }
        public string? MaVach { get; set; }
        public int? TrangThai { get; set; }
        public int? MaSanPham { get; set; }
        public int? MaKichThuoc { get; set; }
        public int? MaMau { get; set; }
    }
}