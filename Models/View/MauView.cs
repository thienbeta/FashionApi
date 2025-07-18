namespace FashionApi.Models.View
{
    public class MauView
    {
        public int MaMau { get; set; }
        public string? TenMau { get; set; }
        public string? MoTa { get; set; }
        public string CodeMau { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int TrangThai { get; set; } // 1: Kích hoạt, 0: Vô hiệu hóa
    }
}
