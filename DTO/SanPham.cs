namespace FashionApi.DTO
{
    public class SanPham
    {
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public decimal GiaBan { get; set; }
        public decimal GiaNhap { get; set; }
        public int SoLuongNhap { get; set; }
        public int SoLuongBan { get; set; }
        public decimal SoLuongSale { get; set; }
        public double PhanTramSale { get; set; }
        public DateTime? NgayBatDauSale { get; set; }
        public DateTime? NgayKetThucSale { get; set; }
        public decimal KhoiLuong { get; set; }
        public string Slug { get; set; }
        public string ChatLieu { get; set; }
        public int GioiTinh { get; set; } // 0 mặc đinh, 1: Nam, 2: Nữ, 3: khác
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int TrangThai { get; set; } = 1; // 1: Kích hoạt, 0: Vô hiệu hóa

        public int MaKichThuoc { get; set; }
        public virtual KichThuoc KichThuocNavigation { get; set; }

        public int MaMau { get; set; }
        public virtual Mau MauNavigation { get; set; }

        public int MaThuongHieu { get; set; }
        public virtual ThuongHieu ThuongHieuNavigation { get; set; }

        public int MaLoai{ get; set; }
        public virtual Loai LoaiNavigation { get; set; }

        public int? MaHashtag { get; set; }
        public virtual Hashtag? HashtagNavigation { get; set; }

        public virtual ICollection<Media> Medias { get; set; } = new List<Media>();
    }
}
