namespace FashionApi.Models.Edit
{
    public class LoaiEdit
    {
        public int MaLoai { get; set; }
        public string TenLoai { get; set; }
        public string? MoTa { get; set; }
        public string KiHieu { get; set; }
        public string? HinhAnh { get; set; }
        public int TrangThai { get; set; } // 1: Kích hoạt, 0: Vô hiệu hóa
    }
}
