namespace FashionApi.Models.Edit
{
    public class BinhLuanEdit
    {
        public int MaBinhLuan { get; set; }
        public string? TieuDe { get; set; }
        public string? NoiDung { get; set; }
        public int? DanhGia { get; set; }

        public List<IFormFile>? Images { get; set; }  // Danh sách hình ảnh tải lên

        public int? TrangThai { get; set; } // Cập nhật trạng thái
    }
}
