using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class AddAdminAndStaffNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThongBao_DocGia_MaDocGia",
                table: "ThongBao");

            migrationBuilder.DropIndex(
                name: "IX_ThongBao_MaDocGia_MaSuKien",
                table: "ThongBao");

            migrationBuilder.AlterColumn<int>(
                name: "MaDocGia",
                table: "ThongBao",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DoiTuong",
                table: "ThongBao",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "LaThongBaoAdmin",
                table: "ThongBao",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaBanTin",
                table: "ThongBao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaNhanVien",
                table: "ThongBao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoNguoiNhan",
                table: "ThongBao",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaDocGia_MaSuKien",
                table: "ThongBao",
                columns: new[] { "MaDocGia", "MaSuKien" },
                unique: true,
                filter: "[MaDocGia] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaNhanVien_MaSuKien",
                table: "ThongBao",
                columns: new[] { "MaNhanVien", "MaSuKien" },
                unique: true,
                filter: "[MaNhanVien] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ThongBao_DocGia_MaDocGia",
                table: "ThongBao",
                column: "MaDocGia",
                principalTable: "DocGia",
                principalColumn: "MaDocGia");

            migrationBuilder.AddForeignKey(
                name: "FK_ThongBao_NhanVien_MaNhanVien",
                table: "ThongBao",
                column: "MaNhanVien",
                principalTable: "NhanVien",
                principalColumn: "MaNhanVien",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThongBao_DocGia_MaDocGia",
                table: "ThongBao");

            migrationBuilder.DropForeignKey(
                name: "FK_ThongBao_NhanVien_MaNhanVien",
                table: "ThongBao");

            migrationBuilder.DropIndex(
                name: "IX_ThongBao_MaDocGia_MaSuKien",
                table: "ThongBao");

            migrationBuilder.DropIndex(
                name: "IX_ThongBao_MaNhanVien_MaSuKien",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "DoiTuong",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "LaThongBaoAdmin",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "MaBanTin",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "MaNhanVien",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "SoNguoiNhan",
                table: "ThongBao");

            migrationBuilder.AlterColumn<int>(
                name: "MaDocGia",
                table: "ThongBao",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaDocGia_MaSuKien",
                table: "ThongBao",
                columns: new[] { "MaDocGia", "MaSuKien" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ThongBao_DocGia_MaDocGia",
                table: "ThongBao",
                column: "MaDocGia",
                principalTable: "DocGia",
                principalColumn: "MaDocGia",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
