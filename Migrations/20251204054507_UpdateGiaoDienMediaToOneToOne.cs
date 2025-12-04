using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGiaoDienMediaToOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Medias_MaGiaoDien",
                table: "Medias");

            migrationBuilder.AlterColumn<int>(
                name: "MaGiaoDien",
                table: "Medias",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medias_MaGiaoDien",
                table: "Medias",
                column: "MaGiaoDien",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Medias_MaGiaoDien",
                table: "Medias");

            migrationBuilder.AlterColumn<int>(
                name: "MaGiaoDien",
                table: "Medias",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Medias_MaGiaoDien",
                table: "Medias",
                column: "MaGiaoDien");
        }
    }
}
