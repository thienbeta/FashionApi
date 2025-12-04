using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionApi.DTO
{
    public class SanPham
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Mã sản phẩm phải lớn hơn 0")]
        public int MaSanPham { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string TenSanPham { get; set; } = null!;

        [StringLength(10000, ErrorMessage = "Mô tả không được vượt quá 10,000 ký tự")]
        public string? MoTa { get; set; }

        [StringLength(200, ErrorMessage = "Slug không được vượt quá 200 ký tự")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug chỉ chứa chữ thường, số và dấu gạch nối")]
        public string Slug { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Chất liệu không được vượt quá 100 ký tự")]
        public string ChatLieu { get; set; } = null!;

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [Range(0, 1, ErrorMessage = "Trạng thái phải là 0 hoặc 1")]
        public int TrangThai { get; set; } = 1; // 1: Kích hoạt, 0: Vô hiệu hóa

        [Range(0, 3, ErrorMessage = "Giới tính phải từ 0 đến 3")]
        public int GioiTinh { get; set; } // 0: Mặc định, 1: Nam, 2: Nữ, 3: Khác

        public int MaLoai { get; set; }
        public int MaThuongHieu { get; set; }
        public int? MaHashtag { get; set; }

        public virtual DanhMuc DanhMucLoai { get; set; } = null!;
        public virtual DanhMuc DanhMucThuongHieu { get; set; } = null!;
        public virtual DanhMuc? DanhMucHashtag { get; set; }

        public virtual ICollection<Media> Medias { get; set; } = new List<Media>();

        [NotMapped]
        public IEnumerable<Media> MediaHinhAnh => Medias?.Where(m => m.LoaiMedia == "image") ?? [];


        public virtual ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();
    }
}