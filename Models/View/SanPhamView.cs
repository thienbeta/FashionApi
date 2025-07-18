namespace FashionApi.Models.View
{
    public class SanPhamView
    {
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public decimal GiaBan { get; set; }
        public decimal GiaNhap { get; set; }
        public int SoLuongNhap { get; set; }
        public int SoLuongBan { get; set; }
        public decimal SoLuongSale { get; set; }
        public double PhanTramSale { get; set; }
        public string Slug { get; set; }
        public string ChatLieu { get; set; }
        public int GioiTinh { get; set; } // 0 mặc đinh, 1: Nam, 2: Nữ, 3: khác
        public DateTime NgayTao { get; set; }
        public int TrangThai { get; set; } // 1: Kích hoạt, 0: Vô hiệu hóa

        public int MaKichThuoc { get; set; }
        public int MaMau { get; set; }
        public int MaThuongHieu { get; set; }
        public int MaLoai { get; set; }
        public int? MaHashtag { get; set; }

        public string? TenKichThuoc { get; set; }
        public string? TenMau { get; set; }
        public string? TenThuongHieu { get; set; }
        public string? TenLoai { get; set; }
        public string? TenHashtag { get; set; }

        public List<MediaView> Medias { get; set; } = new();
    }
}
