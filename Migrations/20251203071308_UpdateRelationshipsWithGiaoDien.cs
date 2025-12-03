using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationshipsWithGiaoDien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaGiaoDien",
                table: "Medias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GiaoDiens",
                columns: table => new
                {
                    MaGiaoDien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenGiaoDien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoaiGiaoDien = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    TrangThai = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDiens", x => x.MaGiaoDien);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medias_MaGiaoDien",
                table: "Medias",
                column: "MaGiaoDien");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDiens_LoaiGiaoDien",
                table: "GiaoDiens",
                column: "LoaiGiaoDien");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDiens_TenGiaoDien",
                table: "GiaoDiens",
                column: "TenGiaoDien");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_GiaoDien",
                table: "Medias",
                column: "MaGiaoDien",
                principalTable: "GiaoDiens",
                principalColumn: "MaGiaoDien",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_GiaoDien",
                table: "Medias");

            migrationBuilder.DropTable(
                name: "GiaoDiens");

            migrationBuilder.DropIndex(
                name: "IX_Medias_MaGiaoDien",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "MaGiaoDien",
                table: "Medias");
        }
    }
}
