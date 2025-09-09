using MimeKit.Cryptography;

namespace FashionApi.Models.View
{
    public class SanPhamView
    {
        public int MaSanPham { get; set; }

        public string TenSanPham { get; set; }

        public string? MoTa { get; set; }

        public string Slug { get; set; }

        public string ChatLieu { get; set; }

        public DateTime NgayTao { get; set; }

        public int TrangThai { get; set; } // 1: Kích hoạt, 0: Vô hiệu hóa

        public int GioiTinh { get; set; } // 0: Mặc định, 1: Nam, 2: Nữ, 3: Khác

        public int MaLoai { get; set; }
        public string? TenLoai { get; set; } // Tên loại sản phẩm để hiển thị
        public string? HinhAnhLoai { get; set; } // Hình ảnh loại sản phẩm để hiển thị

        public int MaThuongHieu { get; set; }
        public string? TenThuongHieu { get; set; } // Tên thương hiệu để hiển thị
        public string? HinhAnhThuongHieu { get; set; } // Hình ảnh thương hiệu để hiển thị

        public int? MaHashtag { get; set; }
        public string? TenHashtag { get; set; } // Tên hashtag để hiển thị
        public string? HinhAnhHashtag { get; set; } // Hình ảnh hashtag để hiển thị

        public List<MediaView> Medias { get; set; } = new List<MediaView>();

        public List<BienTheView> BienThes { get; set; } = new List<BienTheView>();

        public int? TongSoLuongBan { get; set; } // Tổng số lượng bán từ tất cả các biến thể
        public int? TongSoLuongNhap { get; set; } // Tổng số lượng nhập từ tất cả các biến thể

        public decimal? DanhGiaTrungBinh { get; set; } // Đánh giá trung bình từ tất cả các bình luận
        public int? SoLuongDanhGia { get; set; } // Số lượng đánh giá từ tất cả các bình luận

    }
}
