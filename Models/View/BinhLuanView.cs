namespace FashionApi.Models.View
{
    public class BinhLuanView
    {
        public int MaBinhLuan { get; set; }
        public string? TieuDe { get; set; }
        public string? NoiDung { get; set; }
        public int? DanhGia { get; set; }
        public DateTime NgayTao { get; set; }
        public int TrangThai { get; set; }
        public decimal? DanhGiaTrungBinh { get; set; } // Đánh giá trung bình từ tất cả các bình luận
        public int? SoLuongDanhGia { get; set; } // Số lượng đánh giá từ tất cả các bình luận
        public int? SoLuong5Sao { get; set; } // Số lượng đánh giá 5 sao
        public int? SoLuong4Sao { get; set; } // Số lượng đánh giá 4 sao
        public int? SoLuong3Sao { get; set; } // Số lượng đánh giá 3 sao
        public int? SoLuong2Sao { get; set; } // Số lượng đánh giá 2 sao
        public int? SoLuong1Sao { get; set; } // Số lượng đánh giá 1 sao

        public int? MaNguoiDung { get; set; }
        public string? HoTen { get; set; } // Tên người dùng để hiển thị
        public string? Avt { get; set; } // Ảnh đại diện người dùng để hiển thị

        public int? MaSanPham { get; set; }
        public string? TenSanPham { get; set; } // Tên sản phẩm để hiển thị


        public List<MediaView> Medias { get; set; } = new List<MediaView>();
    }
}
