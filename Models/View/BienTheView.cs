namespace FashionApi.Models.View
{
    public class BienTheView
    {
        public int MaBienThe { get; set; }
        public string? HinhAnh { get; set; }
        public decimal GiaBan { get; set; }
        public decimal GiaNhap { get; set; }
        public int SoLuongNhap { get; set; }
        public int SoLuongBan { get; set; }
        public decimal KhuyenMai { get; set; }
        public string? MaVach { get; set; }
        public DateTime NgayTao { get; set; }
        public int TrangThai { get; set; }
        public int MaSanPham { get; set; }
        public decimal GiaTri { get; set; }
        public int MaKichThuoc { get; set; }
        public string? TenKichThuoc { get; set; }
        public string? HinhAnhKichThuoc { get; set; }

        public int MaMau { get; set; }
        public string? TenMau { get; set; }
        public string? HinhAnhMau { get; set; }
    }
}
