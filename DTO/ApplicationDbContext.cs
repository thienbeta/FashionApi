using FashionApi.DTO;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

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
        public DbSet<ThuongHieu> ThuongHieus { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<KichThuoc> KichThuocs { get; set; }
        public DbSet<Loai> Loais { get; set; }
        public DbSet<Mau> Maus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SanPham
            modelBuilder.Entity<SanPham>(entity =>
            {
                entity.HasKey(e => e.MaSanPham);

                entity.Property(e => e.MaSanPham)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.TenSanPham)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.MoTa)
                      .HasMaxLength(1000);

                entity.Property(e => e.HinhAnh)
                      .HasMaxLength(500);

                entity.Property(e => e.GiaBan)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(e => e.GiaNhap)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.SoLuongNhap);
                entity.Property(e => e.SoLuongBan)
                      .IsRequired();

                entity.Property(e => e.SoLuongSale);
                entity.Property(e => e.PhanTramSale);

                entity.Property(e => e.Slug)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.ChatLieu)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.GioiTinh);

                entity.Property(e => e.NgayTao)
                      .HasDefaultValueSql("getutcdate()");

                entity.Property(e => e.TrangThai)
                      .HasDefaultValue(1);

                // relationships
                entity.HasOne(e => e.KichThuocNavigation)
                      .WithMany(k => k.SanPhams)
                      .HasForeignKey(e => e.MaKichThuoc)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MauNavigation)
                      .WithMany(m => m.SanPhams)
                      .HasForeignKey(e => e.MaMau)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ThuongHieuNavigation)
                      .WithMany(t => t.SanPhams)
                      .HasForeignKey(e => e.MaThuongHieu)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LoaiNavigation)
                      .WithMany(l => l.SanPhams)
                      .HasForeignKey(e => e.MaLoai)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.HashtagNavigation)
                      .WithMany(h => h.SanPhams)
                      .HasForeignKey(e => e.MaHashtag)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Medias)
                      .WithOne(m => m.SanPhamNavigation)
                      .HasForeignKey(m => m.MaSanPham);
            });

            // Media
            modelBuilder.Entity<Media>(entity =>
            {
                entity.HasKey(e => e.MaMedia);

                entity.Property(e => e.LoaiMedia)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.DuongDan)
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(e => e.AltMedia)
                      .HasMaxLength(200);

                entity.Property(e => e.NgayTao)
                      .HasDefaultValueSql("getutcdate()");

                entity.Property(e => e.TrangThai)
                      .HasDefaultValue(1);
            });

            // ThuongHieu
            modelBuilder.Entity<ThuongHieu>(entity =>
            {
                entity.HasKey(e => e.MaThuongHieu);

                entity.Property(e => e.TenThuongHieu)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.MoTa)
                      .HasMaxLength(50);

                entity.Property(e => e.HinhAnh)
                      .HasMaxLength(500);

                entity.Property(e => e.NgayTao)
                      .HasDefaultValueSql("getutcdate()");

                entity.Property(e => e.TrangThai)
                      .HasDefaultValue(1);
            });

            // Hashtag
            modelBuilder.Entity<Hashtag>(entity =>
            {
                entity.HasKey(e => e.MaHashtag);

                entity.Property(e => e.TenHashtag)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.MoTa)
                      .HasMaxLength(50);

                entity.Property(e => e.HinhAnh)
                      .HasMaxLength(500);

                entity.Property(e => e.NgayTao)
                      .HasDefaultValueSql("getutcdate()");

                entity.Property(e => e.TrangThai)
                      .HasDefaultValue(1);
            });

            // KichThuoc
            modelBuilder.Entity<KichThuoc>(entity =>
            {
                entity.HasKey(e => e.MaKichThuoc);

                entity.Property(e => e.TenKichThuoc)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.MoTa)
                      .HasMaxLength(50);

                entity.Property(e => e.HinhAnh)
                      .HasMaxLength(500);

                entity.Property(e => e.NgayTao)
                      .HasDefaultValueSql("getutcdate()");

                entity.Property(e => e.TrangThai)
                      .HasDefaultValue(1);
            });

            // Loai
            modelBuilder.Entity<Loai>(entity =>
            {
                entity.HasKey(e => e.MaLoai);

                entity.Property(e => e.TenLoai)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.KiHieu)
                      .HasMaxLength(2)
                      .IsRequired();

                entity.Property(e => e.MoTa)
                      .HasMaxLength(50);

                entity.Property(e => e.HinhAnh)
                      .HasMaxLength(500);

                entity.Property(e => e.NgayTao)
                      .HasDefaultValueSql("getutcdate()");

                entity.Property(e => e.TrangThai)
                      .HasDefaultValue(1);
            });
            //  Mau
            modelBuilder.Entity<Mau>(entity =>
            {
                entity.HasKey(e => e.MaMau);

                entity.Property(e => e.TenMau)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.MoTa)
                      .HasMaxLength(50);

                entity.Property(e => e.HinhAnh)
                      .HasMaxLength(500);

                entity.Property(e => e.NgayTao)
                      .HasDefaultValueSql("getutcdate()");

                entity.Property(e => e.TrangThai)
                      .HasDefaultValue(1);
            });
        }
    }
}
