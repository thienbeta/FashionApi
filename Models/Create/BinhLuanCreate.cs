namespace FashionApi.Models.Create
{
    public class BinhLuanCreate
    {
        public string? TieuDe { get; set; }
        public string? NoiDung { get; set; }
        public int? DanhGia { get; set; }
        public int TrangThai { get; set; } = 1; // Mặc định trạng thái là 1 (hoạt động)

        public int MaNguoiDung { get; set; }
        public int? MaSanPham { get; set; }

        public List<IFormFile>? Images { get; set; }  // Danh sách hình ảnh tải lên
    }
}
