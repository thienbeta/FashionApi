using FashionApi.DTO;
using Microsoft.EntityFrameworkCore;

namespace FashionApi.Data
{
      public class ApplicationDbContext : DbContext
      {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public DbSet<SanPham> SanPhams { get; set; }
            public DbSet<Media> Medias { get; set; }
            public DbSet<DanhMuc> DanhMucs { get; set; }
            public DbSet<NguoiDung> NguoiDungs { get; set; }
            public DbSet<BinhLuan> BinhLuans { get; set; }
            public DbSet<GiaoDien> GiaoDiens { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  // SanPham
                  modelBuilder.Entity<SanPham>(entity =>
                  {
                        entity.HasKey(e => e.MaSanPham);

                        entity.Property(e => e.MaSanPham)
                        .ValueGeneratedOnAdd();

                        entity.Property(e => e.TenSanPham)
                        .HasMaxLength(200)
                        .IsRequired();

                        entity.Property(e => e.MoTa)
                        .HasColumnType("text");

                        entity.Property(e => e.Slug)
                        .HasMaxLength(200)
                        .IsRequired();

                        entity.Property(e => e.ChatLieu)
                        .HasMaxLength(100)
                        .IsRequired();

                        entity.Property(e => e.GioiTinh)
                        .HasDefaultValue(0);

                        entity.Property(e => e.NgayTao)
                        .HasDefaultValueSql("getutcdate()");

                        entity.Property(e => e.TrangThai)
                        .HasDefaultValue(1);

                        entity.Property(e => e.MaLoai)
                        .IsRequired();

                        entity.Property(e => e.MaThuongHieu)
                        .IsRequired();

                        entity.Property(e => e.MaHashtag)
                        .IsRequired(false);

                        // Quan hệ với DanhMuc
                        entity.HasOne(e => e.DanhMucLoai)
                        .WithMany()
                        .HasForeignKey(e => e.MaLoai)
                        .HasConstraintName("FK_SanPham_DanhMuc_Loai")
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(e => e.DanhMucThuongHieu)
                        .WithMany()
                        .HasForeignKey(e => e.MaThuongHieu)
                        .HasConstraintName("FK_SanPham_DanhMuc_ThuongHieu")
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(e => e.DanhMucHashtag)
                        .WithMany()
                        .HasForeignKey(e => e.MaHashtag)
                        .HasConstraintName("FK_SanPham_DanhMuc_Hashtag")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired(false);

                        // Index cho tìm kiếm nhanh
                        entity.HasIndex(e => e.Slug)
                        .IsUnique();
                        entity.HasIndex(e => e.TenSanPham);
                  });



                  // DanhMuc
                  modelBuilder.Entity<DanhMuc>(entity =>
                  {
                        entity.HasKey(e => e.MaDanhMuc);

                        entity.Property(e => e.MaDanhMuc)
                        .ValueGeneratedOnAdd();

                        entity.Property(e => e.TenDanhMuc)
                        .HasMaxLength(50)
                        .IsRequired();

                        entity.Property(e => e.LoaiDanhMuc)
                        .IsRequired();

                        entity.Property(e => e.HinhAnh)
                        .HasMaxLength(500);

                        entity.Property(e => e.NgayTao)
                        .HasDefaultValueSql("getutcdate()");

                        entity.Property(e => e.TrangThai)
                        .HasDefaultValue(1);

                        // Navigation property cho mối quan hệ 1 danh mục - nhiều sản phẩm
                        entity.HasMany(e => e.SanPhams)
                        .WithOne(sp => sp.DanhMucLoai)
                        .HasForeignKey(sp => sp.MaLoai)
                        .HasConstraintName("FK_SanPham_DanhMuc_Loai")
                        .OnDelete(DeleteBehavior.Restrict);

                        // Index cho tìm kiếm nhanh
                        entity.HasIndex(e => e.TenDanhMuc);
                  });

                  // Media (cập nhật lại cho đầy đủ FK với BinhLuan và SanPham)
                  modelBuilder.Entity<Media>(entity =>
                  {
                        entity.HasKey(e => e.MaMedia);

                        entity.Property(e => e.MaMedia)
                        .ValueGeneratedOnAdd();

                        entity.Property(e => e.LoaiMedia)
                        .HasMaxLength(50)
                        .IsRequired();

                        entity.Property(e => e.DuongDan)
                        .HasMaxLength(500)
                        .IsRequired();

                        entity.Property(e => e.AltMedia)
                        .HasMaxLength(200);

                        entity.Property(e => e.LinkMedia)
                        .HasMaxLength(500);

                        entity.Property(e => e.NgayTao)
                        .HasDefaultValueSql("getutcdate()");

                        entity.Property(e => e.TrangThai)
                        .HasDefaultValue(1);

                        // Quan hệ với SanPham (1 sp - nhiều media) → KHÔNG cascade
                        entity.HasOne(e => e.SanPhamNavigation)
                        .WithMany(sp => sp.Medias)
                        .HasForeignKey(e => e.MaSanPham)
                        .HasConstraintName("FK_Media_SanPham")
                        .OnDelete(DeleteBehavior.Restrict);

                        // Quan hệ với BinhLuan (1 bình luận - nhiều media) → Có cascade
                        entity.HasOne(e => e.BinhLuanNavigation)
                        .WithMany(bl => bl.Medias)
                        .HasForeignKey(e => e.MaBinhLuan)
                        .HasConstraintName("FK_Media_BinhLuan")
                        .OnDelete(DeleteBehavior.Cascade);

                        // Quan hệ với GiaoDien (1-1: 1 giao diện có 1 media)
                        entity.HasOne(e => e.GiaoDienNavigation)
                        .WithOne(gd => gd.Media)
                        .HasForeignKey<Media>(e => e.MaGiaoDien)
                        .HasConstraintName("FK_Media_GiaoDien")
                        .OnDelete(DeleteBehavior.Cascade);
                  });


                  // NguoiDung
                  modelBuilder.Entity<NguoiDung>(entity =>
                  {
                        entity.HasKey(e => e.MaNguoiDung);

                        entity.Property(e => e.MaNguoiDung)
                        .ValueGeneratedOnAdd();

                        entity.Property(e => e.HoTen)
                        .HasMaxLength(100)
                        .IsRequired();

                        entity.Property(e => e.NgaySinh)
                        .HasColumnType("date");

                        entity.Property(e => e.Sdt)
                        .HasMaxLength(20);

                        entity.Property(e => e.Email)
                        .HasMaxLength(100)
                        .IsRequired();

                        entity.Property(e => e.TaiKhoan)
                        .HasMaxLength(50)
                        .IsRequired();

                        entity.Property(e => e.MatKhau)
                        .HasMaxLength(100)
                        .IsRequired();

                        entity.Property(e => e.VaiTro)
                        .HasDefaultValue(0);

                        entity.Property(e => e.TrangThai)
                        .HasDefaultValue(1);

                        entity.Property(e => e.Avt)
                        .HasMaxLength(500);

                        entity.Property(e => e.TieuSu)
                        .HasMaxLength(1000);

                        entity.Property(e => e.NgayTao)
                        .HasDefaultValueSql("getutcdate()");

                        entity.Property(e => e.TimeKhoa);

                        entity.Property(e => e.GioiTinh)
                        .HasDefaultValue(0);

                        // Index cho tìm kiếm nhanh
                        entity.HasIndex(e => e.Email)
                        .IsUnique();
                        entity.HasIndex(e => e.TaiKhoan)
                        .IsUnique();
                  });

                  //BinhLuan
                  modelBuilder.Entity<BinhLuan>(entity =>
                  {
                        entity.HasKey(e => e.MaBinhLuan);

                        entity.Property(e => e.MaBinhLuan)
                        .ValueGeneratedOnAdd();

                        entity.Property(e => e.TieuDe)
                        .HasMaxLength(200);

                        entity.Property(e => e.NoiDung)
                        .HasColumnType("text");

                        entity.Property(e => e.NgayTao)
                        .HasDefaultValueSql("getutcdate()");

                        entity.Property(e => e.TrangThai)
                        .HasDefaultValue(1);

                        // Quan hệ với SanPham (1 sản phẩm - nhiều bình luận)
                        entity.HasOne(e => e.SanPhamNavigation)
                        .WithMany(sp => sp.BinhLuans)
                        .HasForeignKey(e => e.MaSanPham)
                        .HasConstraintName("FK_BinhLuan_SanPham")
                        .OnDelete(DeleteBehavior.Cascade);

                        // Quan hệ với NguoiDung (1 user - nhiều bình luận)
                        entity.HasOne(e => e.NguoiDungNavigation)
                        .WithMany(nd => nd.BinhLuans)
                        .HasForeignKey(e => e.MaNguoiDung)
                        .HasConstraintName("FK_BinhLuan_NguoiDung")
                        .OnDelete(DeleteBehavior.Cascade);

                        // Quan hệ với Media (1 bình luận - nhiều media)
                        entity.HasMany(e => e.Medias)
                        .WithOne(m => m.BinhLuanNavigation)
                        .HasForeignKey(m => m.MaBinhLuan)
                        .HasConstraintName("FK_Media_BinhLuan")
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  // GiaoDien
                  modelBuilder.Entity<GiaoDien>(entity =>
                  {
                        entity.HasKey(e => e.MaGiaoDien);

                        entity.Property(e => e.MaGiaoDien)
                        .ValueGeneratedOnAdd();

                        entity.Property(e => e.TenGiaoDien)
                        .HasMaxLength(100)
                        .IsRequired();

                        entity.Property(e => e.LoaiGiaoDien)
                        .IsRequired();

                        entity.Property(e => e.MoTa)
                        .HasMaxLength(500);

                        entity.Property(e => e.MetaTitle)
                        .HasMaxLength(200);

                        entity.Property(e => e.MetaDescription)
                        .HasMaxLength(500);

                        entity.Property(e => e.MetaKeywords)
                        .HasMaxLength(500);

                        entity.Property(e => e.NgayTao)
                        .HasDefaultValueSql("getutcdate()");

                        entity.Property(e => e.TrangThai)
                        .HasDefaultValue(1);

                        // Quan hệ với Media (1-1: 1 giao diện - 1 media)
                        entity.HasOne(e => e.Media)
                        .WithOne(m => m.GiaoDienNavigation)
                        .HasForeignKey<Media>(m => m.MaGiaoDien)
                        .HasConstraintName("FK_Media_GiaoDien")
                        .OnDelete(DeleteBehavior.Cascade);

                        // Index cho tìm kiếm nhanh
                        entity.HasIndex(e => e.TenGiaoDien);
                        entity.HasIndex(e => e.LoaiGiaoDien);
                  });

            }
      }
}
