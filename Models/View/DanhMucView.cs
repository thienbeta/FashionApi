namespace FashionApi.Models.View
{
    public class DanhMucView
    {
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
        public int LoaiDanhMuc { get; set; } // 1: Loại, 2: Thương hiệu, 3: Hashtag, 4: Kích thước, 5: Màu sắc
        public string? HinhAnh { get; set; }
        public DateTime NgayTao { get; set; }
        public int TrangThai { get; set; } // 1: Kích hoạt, 0: Vô hiệu hóa

        public List<SanPhamView> SanPhams { get; set; } = new List<SanPhamView>();
    }
}
