using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class AddUniqueIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NhanVien_UserId",
                table: "NhanVien");

            migrationBuilder.DropIndex(
                name: "IX_DocGia_UserId",
                table: "DocGia");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_UserId",
                table: "NhanVien",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocGia_UserId",
                table: "DocGia",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BanSao_MaVach",
                table: "BanSao",
                column: "MaVach",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NhanVien_UserId",
                table: "NhanVien");

            migrationBuilder.DropIndex(
                name: "IX_DocGia_UserId",
                table: "DocGia");

            migrationBuilder.DropIndex(
                name: "IX_BanSao_MaVach",
                table: "BanSao");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_UserId",
                table: "NhanVien",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocGia_UserId",
                table: "DocGia",
                column: "UserId");
        }
    }
}
