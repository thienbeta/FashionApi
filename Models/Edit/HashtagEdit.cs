namespace FashionApi.Models.Edit
{
    public class HashtagEdit
    {
        public int MaHashtag { get; set; }
        public string TenHashtag { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public int TrangThai { get; set; } // 1: Kích hoạt, 0: Vô hiệu hóa
    }
}
