using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBienThe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BienThes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BienThes",
                columns: table => new
                {
                    MaBienThe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKichThuoc = table.Column<int>(type: "int", nullable: false),
                    MaMau = table.Column<int>(type: "int", nullable: false),
                    MaSanPham = table.Column<int>(type: "int", nullable: true),
                    GiaBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaNhap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KhuyenMai = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    MaVach = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    SoLuongBan = table.Column<int>(type: "int", nullable: false),
                    SoLuongNhap = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BienThes", x => x.MaBienThe);
                    table.ForeignKey(
                        name: "FK_BienThe_DanhMuc_KichThuoc",
                        column: x => x.MaKichThuoc,
                        principalTable: "DanhMucs",
                        principalColumn: "MaDanhMuc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BienThe_DanhMuc_Mau",
                        column: x => x.MaMau,
                        principalTable: "DanhMucs",
                        principalColumn: "MaDanhMuc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BienThes_SanPhams_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BienThes_MaKichThuoc",
                table: "BienThes",
                column: "MaKichThuoc");

            migrationBuilder.CreateIndex(
                name: "IX_BienThes_MaMau",
                table: "BienThes",
                column: "MaMau");

            migrationBuilder.CreateIndex(
                name: "IX_BienThes_MaSanPham",
                table: "BienThes",
                column: "MaSanPham");
        }
    }
}
